using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIP8Library;
using SDL2;
using System.Runtime.InteropServices;
using System.IO;
using System.Timers;
using SimpleSDLWrapperDotNET;
using SimpleSDLWrapperDotNET.Mixer;

/*
 * https://github.com/zyphrus/SharpDL (https://github.com/babelshift/SharpDL)
 */


namespace CHIP8EmulatorTestApp
{
    class Program
    {
        public static int sizeMultiplier = 10;
        private static bool bRunning = true;
        private static AudioSample sample;
        private static int delay = 2;
        private static CHIP8 c8CHIP8 = new CHIP8();

        static void Main(string[] args)
        {
            c8CHIP8.Init();
            string sFileName = string.Empty;
            if (args.Length > 0)
            {
                sFileName = args[0];
            }
            else
            {
                sFileName = @"..\..\..\roms\Zero Demo [zeroZshadow, 2007].ch8";
            }
            string sWindowTitle = new FileInfo(sFileName).Name;
            c8CHIP8.LoadRom(sFileName);
            c8CHIP8.onPlaySound += c8CHIP8_onPlaySound;

            System.Windows.Forms.Form f = new System.Windows.Forms.Form();
            f.Width = CHIP8.GRAPHICS_WIDTH * sizeMultiplier;
            f.Height = CHIP8.GRAPHICS_HEIGHT * sizeMultiplier;


            System.Windows.Forms.MenuStrip menuStrip1 = new System.Windows.Forms.MenuStrip();
            System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(171, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            openToolStripMenuItem,
            exitToolStripMenuItem});
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            openToolStripMenuItem.Text = "Open";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            exitToolStripMenuItem.Text = "Exit";

            f.Controls.Add(menuStrip1);
            f.Height += menuStrip1.Height;


            // set up the graphics
            SDLWrapper sdl = new SDLWrapper();
            sdl.Initialize();
            //sdl.CreateWindow(sWindowTitle, 100, 100, CHIP8.GRAPHICS_WIDTH * sizeMultiplier, CHIP8.GRAPHICS_HEIGHT * sizeMultiplier, WindowFlags.WINDOW_SHOWN);
            sdl.CreateWindowFor(f.Handle);
            sdl.CreateRenderer(SDLWrapper.DEFAULT_RENDERING_DRIVER, RendererFlags.RENDERER_ACCELERATED/* | RendererFlags.RENDERER_PRESENTVSYNC*/);
            //sdl.CreateRendererForWindow(f.Handle, SDLWrapper.DEFAULT_RENDERING_DRIVER, RendererFlags.RENDERER_SOFTWARE);
            Texture txDisplayTexture = sdl.CreateDynamicTexture(CHIP8.GRAPHICS_WIDTH, CHIP8.GRAPHICS_HEIGHT);

            f.Show();

            sdl.OnQuit += sdl_onQuit;
            sdl.OnKeyPressed += sdl_OnKeyPressedOrReleased;
            sdl.OnKeyReleased += sdl_OnKeyPressedOrReleased;

            SDLMixerWrapper mixer = new SDLMixerWrapper();
            mixer.Initialize();
            sample = mixer.LoadSample(@"Ding.wav");

            Color[] palette = new Color[]{
                                           new Color(0, 0, 0, 255),
                                           new Color(128, 255, 0, 255)
                                         };

            // set up the debug windows
            Registers winRegisters = new Registers();
            winRegisters.InitializeRegisters(CHIP8.REGISTERCOUNT);
            winRegisters.Show();


            bRunning = true;
            while (bRunning)
            {
                // call this at the beginning of ever loop to capture the events
                sdl.HandleEvents();

                //sdl.FrameRateCapper();
                System.Threading.Thread.Sleep(delay);

                c8CHIP8.EmulateCycle();
                if (winRegisters.Visible)
                {
                    winRegisters.UpdateRegisters(c8CHIP8.V);
                }

                if (c8CHIP8.GraphicsMemoryChanged)
                {
                    Color[] caPixelPile = new Color[CHIP8.GRAPHICS_WIDTH * CHIP8.GRAPHICS_HEIGHT];
                    for (int i = 0; i < caPixelPile.Length; i++)
                    {
                        caPixelPile[i] = palette[c8CHIP8.GraphicsMemory[i]];
                    }
                    txDisplayTexture.Lock();
                    txDisplayTexture.SetPixels(caPixelPile);
                    txDisplayTexture.Unlock();

                    c8CHIP8.GraphicsMemoryChanged = false;
                }

                sdl.ClearScreen();
                sdl.RenderTexture(txDisplayTexture, null, new Rectangle(0, menuStrip1.Height, 640, 320 - menuStrip1.Height));
                sdl.Render();
            }
        }

        static void c8CHIP8_onPlaySound(object sender, EventArgs e)
        {
            sample.Play();
        }
        static void sdl_OnKeyPressedOrReleased(object sender, KeyboardEventArgs e)
        {
            if (null == e)
            {
                Console.WriteLine("Key pressed: empty key data!!!!");
            }
            else
            {
                Console.WriteLine("Key pressed: code:{0} state:{1} repeat:{2} mod:{3}", e.KeyInfo.Scancode, e.State, e.Repeat, e.KeyInfo.Modifier);
                if (e.State == KeyState.PRESSED)
                {
                    switch (e.KeyInfo.Scancode)
                    {
                        case KeyScancode.D1:
                            c8CHIP8.KeyPad[0x1] = 1;
                            break;
                        case KeyScancode.D2:
                            c8CHIP8.KeyPad[0x2] = 1;
                            break;
                        case KeyScancode.D3:
                            c8CHIP8.KeyPad[0x3] = 1;
                            break;
                        case KeyScancode.D4:
                            c8CHIP8.KeyPad[0xC] = 1;
                            break;
                        case KeyScancode.Q:
                            c8CHIP8.KeyPad[0x4] = 1;
                            break;
                        case KeyScancode.W:
                            c8CHIP8.KeyPad[0x5] = 1;
                            break;
                        case KeyScancode.E:
                            c8CHIP8.KeyPad[0x6] = 1;
                            break;
                        case KeyScancode.R:
                            c8CHIP8.KeyPad[0xD] = 1;
                            break;
                        case KeyScancode.A:
                            c8CHIP8.KeyPad[0x7] = 1;
                            break;
                        case KeyScancode.S:
                            c8CHIP8.KeyPad[0x8] = 1;
                            break;
                        case KeyScancode.D:
                            c8CHIP8.KeyPad[0x9] = 1;
                            break;
                        case KeyScancode.F:
                            c8CHIP8.KeyPad[0xE] = 1;
                            break;
                        case KeyScancode.Y:
                            c8CHIP8.KeyPad[0xA] = 1;
                            break;
                        case KeyScancode.X:
                            c8CHIP8.KeyPad[0x0] = 1;
                            break;
                        case KeyScancode.C:
                            c8CHIP8.KeyPad[0xB] = 1;
                            break;
                        case KeyScancode.V:
                            c8CHIP8.KeyPad[0xF] = 1;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (e.KeyInfo.Scancode)
                    {
                        case KeyScancode.D1:
                            c8CHIP8.KeyPad[0x1] = 0;
                            break;
                        case KeyScancode.D2:
                            c8CHIP8.KeyPad[0x2] = 0;
                            break;
                        case KeyScancode.D3:
                            c8CHIP8.KeyPad[0x3] = 0;
                            break;
                        case KeyScancode.D4:
                            c8CHIP8.KeyPad[0xC] = 0;
                            break;
                        case KeyScancode.Q:
                            c8CHIP8.KeyPad[0x4] = 0;
                            break;
                        case KeyScancode.W:
                            c8CHIP8.KeyPad[0x5] = 0;
                            break;
                        case KeyScancode.E:
                            c8CHIP8.KeyPad[0x6] = 0;
                            break;
                        case KeyScancode.R:
                            c8CHIP8.KeyPad[0xD] = 0;
                            break;
                        case KeyScancode.A:
                            c8CHIP8.KeyPad[0x7] = 0;
                            break;
                        case KeyScancode.S:
                            c8CHIP8.KeyPad[0x8] = 0;
                            break;
                        case KeyScancode.D:
                            c8CHIP8.KeyPad[0x9] = 0;
                            break;
                        case KeyScancode.F:
                            c8CHIP8.KeyPad[0xE] = 0;
                            break;
                        case KeyScancode.Y:
                            c8CHIP8.KeyPad[0xA] = 0;
                            break;
                        case KeyScancode.X:
                            c8CHIP8.KeyPad[0x0] = 0;
                            break;
                        case KeyScancode.C:
                            c8CHIP8.KeyPad[0xB] = 0;
                            break;
                        case KeyScancode.V:
                            c8CHIP8.KeyPad[0xF] = 0;
                            break;
                        default:
                            break;
                    }
                }
                if (e.KeyInfo.Scancode == KeyScancode.NumPadMultiply && e.State == KeyState.PRESSED)
                {
                    sample.Play();
                }
                else if (e.KeyInfo.Scancode == KeyScancode.NumPadPlus && e.State == KeyState.PRESSED)
                {
                    delay++;
                }
                else if (e.KeyInfo.Scancode == KeyScancode.NumPadMinus && e.State == KeyState.PRESSED)
                {
                    delay--;
                }
            }
        }

        static void sdl_onQuit(object sender, EventArgs e)
        {
            bRunning = false;
        }

    }
}
