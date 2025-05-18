# Carriable Objects and Air Vent System Guide

This guide explains how to set up and use the carriable objects system (like apples) and air vents in your game.

## Quick Setup

### Setting up an Air Vent:
1. Create an empty GameObject and name it "AirVent"
2. Add the `AirVent` script component
3. Configure the settings in the Inspector:
   - Accepted object types (e.g., "Apple", "Paper", etc.)
   - Number of required objects to solve the puzzle
   - Whether to activate something when the puzzle is solved

### Creating an Apple:
1. Create an empty GameObject and name it "Apple"
2. Add the `CarriableObjectSetup` component
3. Click the cog icon in the component header and select "Create Apple"
4. A pre-configured apple will be created at the position

## Detailed Components

### CarriableObject
This script handles objects that can be picked up, carried, and dropped into receivers.

Key features:
- Press E to pick up and drop
- Objects automatically drop if the player moves too far away
- Carried objects hover slightly in front of the player
- Objects are highlighted when over a compatible receiver
- Physics is disabled while carrying to prevent collision issues

### ObjectReceiver
Base class for objects that can receive/accept carriable items. Provides:
- Highlight when compatible objects hover over it
- Visual and sound effects when objects are received
- System for accepting only specific object types

### AirVent
Specialized receiver that acts as an air vent:
- Optional suction to pull nearby compatible objects
- Particle effects simulating air flow
- Can unlock or activate other game elements when filled
- Supports puzzle mechanics requiring multiple objects

## Example Usage

### Locked Door Puzzle
1. Create a door in your scene
2. Create an air vent near the door
3. Set the air vent to activate the door GameObject when solved
4. Place a few apples around your level
5. Player needs to find the apples and drop them in the vent to unlock the door

### Creating Custom Carriable Objects
To create a custom carriable object (like a paper airplane):
1. Create a 3D or 2D object with the desired mesh/sprite
2. Add the `CarriableObject` component
3. Set the `objectType` to a unique identifier (e.g., "PaperAirplane")
4. Make sure your air vents include this type in their `acceptedObjectTypes` list

## Tips
- Use the `showDebugInfo` option during development to see interaction ranges
- Adjust the `carryHeight` and `carryDistance` to fine-tune how objects are held
- The `suctionRadius` and `suctionForce` parameters control how strongly the vent pulls objects
- Set `requiredObjectCount` on the air vent to determine how many objects are needed to solve the puzzle
