# Interaction System Setup Guide

This guide will help you set up and use the interaction system in your Unity project.

## Quick Setup

1. Add the `GameSceneSetup` script to an empty GameObject in your scene.
2. Press Play, and the system will automatically:
   - Create a player cube if none exists
   - Set up the interaction UI system
   - Create a score manager

## Manual Setup (Alternative)

### Player Setup:
1. Make sure your player GameObject has the tag "Player"
2. Add the `PlayerController` script to your player

### UI Setup:
1. Create a Canvas in your scene
2. Add a Panel for the prompt display
3. Add a Text object inside the panel
4. Add the `UIPromptController` script to the Canvas
5. Assign the Panel to the "promptPanel" field
6. Assign the Text to the "promptText" field

### Interactive Objects:
1. Create a GameObject to serve as a collectible
2. Add the `CollectibleItem` script to it
3. Configure the scoreValue, destroyOnCollect, and collectEffect fields

## Create Collectibles In-Game

You can use the `CollectibleSetupHelper` to create collectibles:

1. Add the `CollectibleSetupHelper` script to an empty GameObject
2. Right-click on the component header in the Inspector
3. Select "Create Default Collectible" from the context menu

## Score System

The `ScoreManager` handles player score tracking:

1. Create a Canvas for UI if you don't have one
2. Add a Text element named "ScoreText"
3. Create an empty GameObject and add the `ScoreManager` script
4. Assign the Text element to the "scoreText" field

## Test Your Setup

1. Press Play in the Unity Editor
2. Use WASD or arrow keys to move the player
3. Approach a collectible (it should highlight)
4. Press E to interact and collect it
5. Check that the score increases

## Troubleshooting

If objects don't interact properly:
1. Check that the player has the tag "Player"
2. Verify that collectibles have the `CollectibleItem` script attached
3. Make sure `UIPromptController` is set up correctly
4. Check the console for any error messages

## Extending The System

You can create your own interactive objects by:
1. Creating a new script that inherits from `InteractiveObject`
2. Overriding the `Interact()` method with your custom behavior
3. Optionally overriding `Start()` and `Update()` (call base methods)

Example for a custom interactive object:

```csharp
public class MyCustomInteractive : InteractiveObject
{
    protected override void Interact()
    {
        // Your custom interaction code here
        Debug.Log("Custom interaction!");
    }
}
```
