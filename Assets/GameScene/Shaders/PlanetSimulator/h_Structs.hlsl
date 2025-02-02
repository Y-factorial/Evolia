#pragma once

struct Statistics
{
	float temperatureSum;
	float oceanDepthSum;
	float photosynthesisSum;
	float respirationSum;
    uint oceanTileCount;

    float photosynthesisMass;
    float respirationMass;
};

struct Atmosphere
{
	float co2Mass;
	float o2Mass;
	float n2Mass;
	float h2oMass;
	float temperature;
    
	float h2oCapacity;
	float o2Ratio;
};

struct Ocean
{
	float seaLevel;
	float oceanMass;
    
	float mineral;
	float oxydizedMineral;
};

struct Orbit
{
	float solarLuminosity;
	float solarDistance;
	float radius;
	float axisTilt;
    float gravity;
};

struct Planet
{
	Orbit orbit;
	Atmosphere atmosphere;
	Ocean ocean;
    
	float solarLongitude;
	float solarConstant;
	float solarDeclination;
	float radiationForcing;
    
	Statistics statistics;
};

struct Row
{
	float latitude;
	float solarEnergy;
    
    float2 wind;
    int2 upstream1;
	int2 upstream2;
    float up1Weight;
    float windPower;
};

struct Species
{
	uint id;
    
	uint layer;
    uint timescale;
	uint palette;
	float scale;
    uint variantType;
    
    float competitiveness;
    float evolutionSpeed;
	
	float growthSpeed;
    float reproductAt;
    float mobility;
    float densityCapacity;
	
    float produce[5];
    float consume[5];
    
    float photosynthesis;
    float respiration;
    
    float2 preferences[4][4];

	uint4 transforms;
    
	uint uniteWith;
	uint uniteTo;
};

struct Variant
{
    float preferences[4];
    float competitiveness;
};

struct Life
{
	uint species;
	uint variant;
    
    float maturity;
    float health;
	float blessedTick;
};

struct Tile
{
	float temperature;
	float humidity;
	float elevation;
    
    float nutrients[5];
    
    float dTemperature;
    float dHumidity;
    float dElevation;
    
	Life lives[3];
	
};

struct TileDetails
{
    Tile _tile;
    int visibleLayer;
    float happiness[4];
    float densityHappiness;
    float nutrientHappiness;
    float totalHappiness;
};