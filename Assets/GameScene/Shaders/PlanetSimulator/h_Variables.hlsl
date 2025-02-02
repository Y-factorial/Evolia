#pragma once

#include "h_Structs.hlsl"

uint2 size;

float deltaTick;
float geologicalSpeed;
float metabolismSpeed;
float heatSpeed;
float age;
uint seed;
float microbeScale;

RWStructuredBuffer<Planet> planets;
RWStructuredBuffer<Row> rows;
StructuredBuffer<Species> col;
StructuredBuffer<Variant> variants;
RWStructuredBuffer<Tile> tiles;
StructuredBuffer<float> updowns;
RWStructuredBuffer<Statistics> statistics1;
RWStructuredBuffer<Statistics> statistics2;
RWStructuredBuffer<Statistics> statistics3;
RWStructuredBuffer<uint> envStatistics;
RWStructuredBuffer<uint> populations;
RWStructuredBuffer<TileDetails> tileDetails;

uint2 commandPos;
float commandR;
float commandH;
uint commandSpecies;
RWStructuredBuffer<Life> commandLife;
RWStructuredBuffer<int> commandRxy;

RWTexture2D<float4> surfaceTexture;
RWTexture2D<float4> lifeTexture;
RWTexture2D<float4> overlayTexture;

#define GEOLOGICAL_TIMESCALE_SPEED 1000000.0f
#define TILE_SIZE 100000.0f

#define VARIANT_COUNT 12
#define INITIAL_BLESS_TICK 10.0f

#define planet planets[0]
#define row rows[y]
#define tile tiles[xy]
#define ntile tiles[nxy]

#define LAYER_COUNT 3
#define LAYER_MICROBE 0
#define LAYER_PLANT 1
#define LAYER_ANIMAL 2

#define microbe lives[LAYER_MICROBE]
#define plant lives[LAYER_PLANT]
#define animal lives[LAYER_ANIMAL]
#define life lives[layer]
#define nlife lives[nlayer]

#define PREFERENCE_COUNT 4
#define PREFERENCE_O2 0
#define PREFERENCE_ELEVATION 1
#define PREFERENCE_TEMPERATURE 2
#define PREFERENCE_HUMIDITY 3

#define prefO2 preferences[PREFERENCE_O2]
#define prefElevation preferences[PREFERENCE_ELEVATION]
#define prefTemperature preferences[PREFERENCE_TEMPERATURE]
#define prefHumidity preferences[PREFERENCE_HUMIDITY]

#define NUTRIENT_COUNT 5
#define NUTRIENT_SOIL 0
#define NUTRIENT_LEAVES 1
#define NUTRIENT_HONEY 2
#define NUTRIENT_FRUITS 3
#define NUTRIENT_MEATS 4

#define EPSILON 0.0001f

#define commandCenterH commandRxy[0]

#define ENV_STATISTICS_SIZE 64
