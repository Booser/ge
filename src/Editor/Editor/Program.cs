using Engine.Behaviors;
using Engine.Graphics;
using System.Runtime.InteropServices;
using Veldrid.Platform;
using Veldrid.Graphics;
using System.Numerics;
using System.IO;
using System;
using Engine.Physics;
using Engine.Editor;
using Engine.Assets;
using Engine;
using Veldrid.Assets;
using ImGuiNET;
using System.Reflection;

namespace Engine.Editor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            OpenTKWindow window = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? (OpenTKWindow)new DedicatedThreadWindow(960, 540, WindowState.Maximized) 
                : new SameThreadWindow(960, 540, WindowState.Maximized);
            window.Title = "ge.Editor";
            Game game = new Game();
            GraphicsSystem gs = new GraphicsSystem(window);
            game.SystemRegistry.Register(gs);

            InputSystem inputSystem = new InputSystem(window);
            inputSystem.RegisterCallback((input) =>
            {
                if (input.GetKeyDown(Key.F4) && (input.GetKey(Key.AltLeft) || input.GetKey(Key.AltRight)))
                {
                    game.Exit();
                }
            });

            game.SystemRegistry.Register(inputSystem);

            ImGuiRenderer imGuiRenderer = new ImGuiRenderer(gs.Context, window.NativeWindow, inputSystem);
            gs.AddFreeRenderItem(imGuiRenderer);
            ImGui.GetIO().FontAllowUserScaling = true;

            AssetSystem assetSystem = new AssetSystem();
            game.SystemRegistry.Register(assetSystem);

            BehaviorUpdateSystem bus = new BehaviorUpdateSystem(game.SystemRegistry);
            game.SystemRegistry.Register(bus);
            bus.Register(imGuiRenderer);

            PhysicsSystem ps = new PhysicsSystem();
            game.SystemRegistry.Register(ps);

            ConsoleCommandSystem ccs = new ConsoleCommandSystem(game.SystemRegistry);
            game.SystemRegistry.Register(ccs);

            window.Closed += game.Exit;

            var editorSystem = new EditorSystem(game.SystemRegistry);
            editorSystem.DiscoverComponentsFromAssembly(typeof(Program).GetTypeInfo().Assembly);
            // Editor system registers itself.

            // Force-load prefs.
            var prefs = EditorPreferences.Instance;

            game.RunMainLoop();

            EditorPreferences.Instance.Save();
        }
    }
}