using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace StaticInventory
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        private int toolbarOffset = 0;
        private IClickableMenu oldMenu;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            oldMenu = Game1.activeClickableMenu;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.Rendering += this.OnRendering;
        }

        /*********
        ** Private methods
        *********/

        private static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        private void ShiftToolbarOffset(bool right)
        {
            if (right) toolbarOffset = Mod(toolbarOffset + 1, 3);
            else toolbarOffset = Mod(toolbarOffset - 1, 3);
            this.Monitor.Log($"Set toolbarOffset to {toolbarOffset}", LogLevel.Debug);
        }

        private void ShiftInventory(bool right)
        {
            NetObjectList<Item> items = new(Game1.player.Items);
            int offsetAmount = toolbarOffset * 12;

            this.Monitor.Log($"items.Count {items.Count}", LogLevel.Debug);

            if (right)
            {
                List<Item> range = items.GetRange(0, offsetAmount);
                items.RemoveRange(0, offsetAmount);
                items.AddRange(range);
            }
            else
            {
                List<Item> range2 = items.GetRange(items.Count - offsetAmount, offsetAmount);
                for (int i = 0; i < items.Count - offsetAmount; i++)
                {
                    range2.Add(items[i]);
                }

                items.Set(range2);
            }

            Game1.player.Items = items;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // Same logic as Game1._update
            if (Game1.currentLocation == null || Game1.currentMinigame != null || Game1.emoteMenu != null || Game1.textEntry != null || Game1.activeClickableMenu != null || Game1.globalFade || Game1.freezeControls)
            {
                return;
            }

            IList<Item> items = Game1.player.Items;
            bool usingTool = Game1.player.UsingTool;

            // Same logic as Game1.player.shiftToolbar
            if (items == null || items.Count < 12 || usingTool || Game1.dialogueUp || (!Game1.pickingTool && !Game1.player.CanMove) || Game1.player.areAllItemsNull() || Game1.eventUp || Game1.farmEvent != null)
            {
                return;
            }

            KeyboardState currentKBState = Game1.GetKeyboardState();
            GamePadState currentPadState = Game1.input.GetGamePadState();

            // Clicking the toolbar button that was set
            if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.toolbarSwap) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.toolbarSwap))
            {
                ShiftToolbarOffset(!currentKBState.IsKeyDown(Keys.LeftControl));
            }

            if (Game1.options.gamepadControls)
            {
                // Clicking the right shoulder button on controller
                if (currentPadState.IsButtonDown(Buttons.RightShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.RightShoulder))
                {
                    ShiftToolbarOffset(true);
                }

                // Clicking the left shoulder button on controller
                if (currentPadState.IsButtonDown(Buttons.LeftShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.LeftShoulder))
                {
                    ShiftToolbarOffset(false);
                }
            }

            // Debug mode keybindings
            if (!Game1.IsChatting && Game1.player.freezePause <= 0 && Game1.debugMode)
            {
                if (currentKBState.IsKeyDown(Keys.B) && !Game1.oldKBState.IsKeyDown(Keys.B))
                {
                    ShiftToolbarOffset(false);
                }
                if (currentKBState.IsKeyDown(Keys.N) && !Game1.oldKBState.IsKeyDown(Keys.N))
                {
                    ShiftToolbarOffset(true);
                }
            }
        }

        private void OnRendering(object sender, RenderingEventArgs e)
        {
            if (Context.IsWorldReady && toolbarOffset != 0 && oldMenu != Game1.activeClickableMenu)
            {
                this.Monitor.Log($"oldMenu: {oldMenu}", LogLevel.Debug);
                this.Monitor.Log($"newMenu: {Game1.activeClickableMenu}", LogLevel.Debug);

                // If we are opening any menu
                if (Game1.activeClickableMenu != null) ShiftInventory(false);

                // If we are closing any menu
                else if (Game1.activeClickableMenu == null) ShiftInventory(true);
            }

            // Reset 
            oldMenu = Game1.activeClickableMenu;
        }
    }
}