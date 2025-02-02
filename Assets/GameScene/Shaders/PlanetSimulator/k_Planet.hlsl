
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

[numthreads(1, 1, 1)]
void Begin()
{
    // ���z�萔���X�V
    float luminosityFactor;
    if (age < 0.1e9)
    {
        // ��n�񐯂ւ̈ڍs�i�ŏ���1���N�j
        luminosityFactor = 0.7 + 0.3 * age / 0.1e9;
    }
    else if (age < 10e9)
    {
        // ��n�񐯁i100���N�܂Łj
        // 10���N������ 0.007 ���x����������
        luminosityFactor = 1 + (1.07 - 1) * (age - 0.1e9) / (10e9 - 0.1e9);

    }
    else if (age < 10.2e9)
    {
        // �ԐF�����i�K�A��������2���N�Ńs�[�N��
        luminosityFactor = 1.07 * 200 * exp((age - 10e9) / 0.2e9);
    }
    else if (age < 11e9)
    {
        // �ԐF�����i�K�A���̌����
        luminosityFactor = 1.07 * 200;
    }
    else
    {
        // ���F�␯�i�K
        luminosityFactor = 1.07 * 200 * exp(-(age - 11e9) / 1e9);
    }

    planet.solarConstant =  luminosityFactor * planet.orbit.solarLuminosity / (4 * PI * planet.orbit.solarDistance * planet.orbit.solarDistance);
    
    // ���z���o���X�V
    float orbitalPeriod = 360;
    planet.solarLongitude = (planet.solarLongitude + deltaTick * PI * 2 / orbitalPeriod) % (PI * 2);
    
    // ���z�̐Ԉ܂��X�V
    // �n�����X���Ă��Ȃ���Ώ��0
    // �n����90�x�X���Ă���΁A0�`��
    // �A���ܓx�Ȃ̂�0 �ƃ΂͓���
    planet.solarDeclination = asin(sin(planet.orbit.axisTilt) * sin(planet.solarLongitude));

    
    // �������ʂ��v�Z����
    // 280ppm��CO2�̎��ʂ�2.19e15kg
    float EarthCO2Mass0 = 2.19e15; // Kg
    // �n���̒P�ʖʐϓ������CO2�̎���
    float CO2MassPerArea0 = EarthCO2Mass0 / (4 * PI * 6371000.0f * 6371000.0f);
    // �P�ʖʐϓ������CO2����
    float CO2MassPerArea = planet.atmosphere.co2Mass / (4 * PI * planet.orbit.radius * planet.orbit.radius);
    // ���ˋ����͌W�� IPCC (1990)�����Myhre et al. (1998)�ɂ���
    float kCO2 = 5.35f;
    // CO2=CO20�̎��̕��ˋ�����
    float fCO2base = 37.5f;
    // CO2�̕��ˋ�����
    float fCO2 = max(0, kCO2 * log(CO2MassPerArea / CO2MassPerArea0) + fCO2base);
    
    // �Y�Ɗv���O�̐����C��
    float EarthH2OMass0 = 1.27e16; // Kg
    float H2OMassPerArea0 = EarthH2OMass0 / (4 * PI * 6371000.0f * 6371000.0f);
    float H2OMassPerArea = planet.atmosphere.h2oMass / (4 * PI * planet.orbit.radius * planet.orbit.radius);
    // H2O�̕��ˋ����͌W��
    float kH2O = 0.7f;
    float fH2Obase = 75.0f;
    float fH2O = max(0, kH2O * log(H2OMassPerArea / H2OMassPerArea0) + fH2Obase);
    // ���̑��̕��ˋ�����
    float fOther = 37.5f;
    
    // ���ˋ�����
    planet.radiationForcing = fCO2 + fH2O + fOther;
    
    // �_�f�Z�x���v�Z����
    planet.atmosphere.o2Ratio = planet.atmosphere.o2Mass / (planet.atmosphere.o2Mass + planet.atmosphere.co2Mass + planet.atmosphere.n2Mass + planet.atmosphere.h2oMass);
    
    
}



[numthreads(1, 1, 1)]
void End()
{
    planet.atmosphere.temperature = planet.statistics.temperatureSum / (size.x * size.y);

    // CO2 �� 50,000,000,000,000,000,000 ����
    // 100x100�}�X��100�C�������Ă����Ƃ��� 1,000,000
    // 3���N�̋z���ʂ�
    // 300,000,000,000,000
    
    // �������� 1,000ppm �Ń}�b�N�X�A150ppm �Ŏ~�܂�
    
    float metabolismUnit = 100000000; // 100000000 �ɈӖ��͂Ȃ��B���x�����킹�Ă邾��
    
    float atmospherMass = planet.atmosphere.n2Mass + planet.atmosphere.o2Mass + planet.atmosphere.co2Mass + planet.atmosphere.h2oMass;
    
    float C = planet.atmosphere.co2Mass / atmospherMass;
    float Cmin = 150.0f / 1000000; // �Œ�Z�x
    float Ck = 200.0f / 1000000; // ���O�a�萔
    float G0 = planet.statistics.photosynthesisSum * metabolismUnit; 
    
    float O = planet.atmosphere.o2Mass / atmospherMass;
    float Omin = 40000 / 1000000; // �Œ�Z�x 4%
    float Ok = 100.0f / 1000000; // ���O�a�萔
    float R0 = planet.statistics.respirationSum * metabolismUnit;
    
    // ���ۂ̑�ӗʂł͂Ȃ��A��Ӕ\�͂��L�^���邱�Ƃɂ���
    // ���ۂ̑�ӗʂ�CO2�Z�x�̉e���Ō������ƌċz���o�����X���Ă��܂��A�ǂ��炪�D���Ȃ̂������������Ȃ�����
    planet.statistics.photosynthesisMass = G0;
    planet.statistics.respirationMass = R0;
    
    float deltaTime = metabolismSpeed * deltaTick;

    float deltaCO2Mass;
    if (C - Cmin > Ck)
    {
        // CO2�Z�x�������Ƃ��́AG��萔�Ƃ��Ĉ�����
        // C(t) = C(0) - (G-R)*t
        
        float G = G0 * (C - Cmin) / (Ck + (C - Cmin));
        float R = R0 * (O - Omin) / (Ok + (O - Omin));
        
        // �����̌�������
        deltaCO2Mass = clamp((R - G) * deltaTime, (atmospherMass * Cmin - planet.atmosphere.co2Mass), planet.atmosphere.o2Mass * 44 / 32);
    }
    else
    {
        // CO2�Z�x���Ⴂ�Ƃ��́AG �̎��̕���� Ck �ɕς���
        // G = G0 * (C - Cmin) / Ck �Ƃ��A
        // ���t��Ԃ�\�� Ceq = Cmin + R0/G0 * Ck ��p�����
        // C(t) = ( C(0) - Ceq ) * exp(-G0/R * t) + Ceq
        
        float Ceq = (Cmin + R0 / (G0 + EPSILON) * Ck) * atmospherMass; // ���t��Ԃ�CO2����
        deltaCO2Mass = (planet.atmosphere.co2Mass - Ceq) * exp(-G0 / (R0 + EPSILON) * deltaTime) + Ceq - planet.atmosphere.co2Mass;
    }
    planet.atmosphere.co2Mass += deltaCO2Mass;
    planet.atmosphere.o2Mass -= deltaCO2Mass * 32 / 44;

    
    {
        // �Ҍ��I�z�����������A�_�f����C���ɕ��o����邱�Ƃ͖���
        // �z���ʂ͎_�f�Z�x�ɔ��
        float m = clamp(planet.atmosphere.o2Mass * 0.99f, 0, planet.atmosphere.o2Mass / atmospherMass * planet.ocean.mineral);
        planet.ocean.mineral -= m;
        planet.ocean.oxydizedMineral += m;
        planet.atmosphere.o2Mass -= m;
    }
    
    if (planet.atmosphere.o2Ratio > 0.3f)
    {
        // �R�Ύ�����
        float fire = planet.atmosphere.o2Mass * 0.01f;
        planet.atmosphere.co2Mass += fire;
        planet.atmosphere.o2Mass -= fire * 32 / 44;
    }
    
    // �����C��
    // Clausius-Clapeyron���������
    float e0 = 611.2f; // ����x�ɂ�����O�a�����C��(Pa)
    float L = 2.5e6f; // ���̏������M(J/kg)
    float Rv = 461.0f; // �����C�̋C�̒萔(J/kgK)
    float vaporPressure = e0 * exp(L / Rv * (1 / CelsiusZero - 1 / planet.atmosphere.temperature)); // �O�a�����C��(Pa)
    
    // ��C����H2O���e��
    // �P���ɁA���x 50% ���Z
    planet.atmosphere.h2oCapacity = vaporPressure * (4 * PI * planet.orbit.radius * planet.orbit.radius) / planet.orbit.gravity * 0.5;
    
    // ���e�ʂ𒴂������͊C�ɂȂ�
    float condense = max(-planet.ocean.oceanMass, planet.atmosphere.h2oMass - planet.atmosphere.h2oCapacity);
        
    planet.ocean.oceanMass += condense;
    planet.atmosphere.h2oMass -= condense;
    
    float currentOceanMass = planet.statistics.oceanDepthSum * TILE_SIZE * TILE_SIZE * 1000.0f;
    
    float dOceanMass = planet.ocean.oceanMass - currentOceanMass;
    
    // �C�̖ڕW�[�x = �C�m���� / �n���̖ʐ� / �C�m���x
    // ���̊C�̃^�C�����Ŋ���ƃI�[�o�[�V���[�g�̃��X�N������
    // �C����������Ƃ��́A�I�[�o�[�V���[�g���Ă����܂�Q�͂Ȃ��̂ŁA���̂܂܃^�C�����Ŋ���̂��ǂ�
    // �C���ł���Ƃ��́A�I�[�o�[�V���[�g����ƑS�Đ��v���Ă��܂��̂ŁA�f���̑S�ʐςŊ���̂��ǂ�
    float dOceanDepth = dOceanMass / ((dOceanMass < 0 ? max(1, planet.statistics.oceanTileCount) : size.x * size.y ) * TILE_SIZE * TILE_SIZE) / 1000.0f;
    
    // ���̐[�x���ڕW�[�x���󂢏ꍇ�A�C�ʂ��㏸����
    planet.ocean.seaLevel += dOceanDepth;
}



