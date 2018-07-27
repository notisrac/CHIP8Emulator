using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleSDLWrapperDotNET;
using SimpleSDLWrapperDotNET.Mixer;
using CHIP8Library;
using System.IO;

namespace CHIP8Emulator
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMessage
    {
        public IntPtr Handle;
        public uint Message;
        public IntPtr WParameter;
        public IntPtr LParameter;
        public uint Time;
        public Point Location;
    }

    public enum EmulationState
    {
        Stopped,
        Running,
        Paused
    }

    public partial class frmMainForm : Form
    {
        private DebugConsole _dbgConsole = null;
        public DebugConsole dbgConsole
        {
            get
            {
                if (null == _dbgConsole || _dbgConsole.IsDisposed || _dbgConsole.Disposing)
                {
                    _dbgConsole = new DebugConsole();
                }
                return _dbgConsole;
            }
        }

        private DebugWindow _dbgWindow = null;
        public DebugWindow dbgWindow
        {
            get
            {
                if (null == _dbgWindow || _dbgWindow.IsDisposed || _dbgWindow.Disposing)
                {
                    _dbgWindow = new DebugWindow();
                }
                return _dbgWindow;
            }
        }

        private MemoryMap _memMap = null;
        public MemoryMap memMap
        {
            get
            {
                if (null == _memMap || _memMap.IsDisposed || _memMap.Disposing)
                {
                    _memMap = new MemoryMap();
                }
                return _memMap;
            }
        }

        public EmulationState EmuState = EmulationState.Stopped;

        private SDLWrapper _sdlSDLWrapper = new SDLWrapper();
        private int _iWidth = 0;
        private int _iHeight = 0;
        private int _iLeft = 0;
        private int _iTop = 0;
        private Texture _txDisplayTexture = null;
        private Texture _txLogoTexture = null;
        private AudioSample _sampleDing;
        private int delay = 2;
        private CHIP8 c8CHIP8 = new CHIP8();
        private SimpleSDLWrapperDotNET.Color[] _palette;
        private Random _rnd = new Random();

        private bool isRunning = false;
             

        public frmMainForm()
        {
            InitializeComponent();

            // set up the graphics
            _sdlSDLWrapper = new SDLWrapper();
            _sdlSDLWrapper.Initialize();
            //sdl.CreateWindow(sWindowTitle, 100, 100, CHIP8.GRAPHICS_WIDTH * sizeMultiplier, CHIP8.GRAPHICS_HEIGHT * sizeMultiplier, WindowFlags.WINDOW_SHOWN);
            _sdlSDLWrapper.CreateWindowFor(this.Handle);
            _sdlSDLWrapper.CreateRenderer(SDLWrapper.DEFAULT_RENDERING_DRIVER, RendererFlags.RENDERER_ACCELERATED/* | RendererFlags.RENDERER_PRESENTVSYNC*/);
            //sdl.CreateRendererForWindow(f.Handle, SDLWrapper.DEFAULT_RENDERING_DRIVER, RendererFlags.RENDERER_SOFTWARE);
            _txDisplayTexture = _sdlSDLWrapper.CreateDynamicTexture(CHIP8.GRAPHICS_WIDTH, CHIP8.GRAPHICS_HEIGHT);
            _txLogoTexture = _sdlSDLWrapper.CreateTextureFromBMP("CHIP-8.bmp");

            _sdlSDLWrapper.OnKeyPressed += sdl_OnKeyPressedOrReleased;
            _sdlSDLWrapper.OnKeyReleased += sdl_OnKeyPressedOrReleased;

            SDLMixerWrapper mixer = new SDLMixerWrapper();
            mixer.Initialize();
            _sampleDing = mixer.LoadSample(@"Ding.wav");

            Application.Idle += Application_Idle;

            c8CHIP8.Init();
            c8CHIP8.onPlaySound += c8CHIP8_onPlaySound;
            c8CHIP8.onDebugMessage += c8CHIP8_onDebugMessage;

            dbgConsole.Shown += dbgConsole_Shown;
            dbgConsole.FormClosed += dbgConsole_FormClosed;
            dbgConsole.Disposed += dbgConsole_Disposed;

            dbgWindow.Disposed += dbgWindow_Disposed;
            dbgWindow.onStart += dbgWindow_onStart;
            dbgWindow.onStep += dbgWindow_onStep;

            _palette = new SimpleSDLWrapperDotNET.Color[]{
                                           new SimpleSDLWrapperDotNET.Color(0, 0, 0, 255),
                                           new SimpleSDLWrapperDotNET.Color(128, 255, 0, 255)
                                         };

            // init the debug windows
            memMap.UpdateTable(c8CHIP8.Memory);
        }

        void dbgWindow_onStep(object sender, EventArgs e)
        {
            isRunning = true;
        }

        void dbgWindow_onStart(object sender, EventArgs e)
        {
            if (EmuState == EmulationState.Paused)
            {
                isRunning = true;
                EmuState = EmulationState.Running;
            }
            else if (EmuState == EmulationState.Running)
            {
                isRunning = false;
                EmuState = EmulationState.Paused;
            }
        }

        void dbgWindow_Disposed(object sender, EventArgs e)
        {
            dbgWindow.Disposed += dbgWindow_Disposed;
            dbgWindow.onStart += dbgWindow_onStart;
            dbgWindow.onStep += dbgWindow_onStep;
        }

        void dbgConsole_Disposed(object sender, EventArgs e)
        { // HACK need to reassign the event handlers, because when the window is closed it is disposed as well
            dbgConsole.Shown += dbgConsole_Shown;
            dbgConsole.FormClosed += dbgConsole_FormClosed;
            dbgConsole.Disposed += dbgConsole_Disposed;
        }

        void dbgConsole_FormClosed(object sender, FormClosedEventArgs e)
        { // disable debugging
            c8CHIP8.DebugEnabled = false;
        }

        void dbgConsole_Shown(object sender, EventArgs e)
        { // enable debugging
            c8CHIP8.DebugEnabled = true;
        }

        void c8CHIP8_onDebugMessage(object sender, DebugEventArgs e)
        {
            dbgConsole.AddLine(e.Message);
        }

        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        private bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            _iWidth = this.ClientRectangle.Width;
            _iHeight = this.ClientRectangle.Height - msMenuStrip.Height;
            _iLeft = this.ClientRectangle.X;
            _iTop = this.ClientRectangle.Y + msMenuStrip.Height;

            while (IsApplicationIdle())
            {
                // call this at the beginning of ever loop to capture the events
                _sdlSDLWrapper.HandleEvents();

                if (c8CHIP8.ROMLoaded && isRunning)
                {
                    c8CHIP8.EmulateCycle();
                    if (c8CHIP8.GraphicsMemoryChanged)
                    {
                        SimpleSDLWrapperDotNET.Color[] caPixelPile = new SimpleSDLWrapperDotNET.Color[CHIP8.GRAPHICS_WIDTH * CHIP8.GRAPHICS_HEIGHT];
                        for (int i = 0; i < caPixelPile.Length; i++)
                        {
                            caPixelPile[i] = _palette[c8CHIP8.GraphicsMemory[i]];
                            //caPixelPile[i] = new SimpleSDLWrapperDotNET.Color((byte)_rnd.Next(255), (byte)_rnd.Next(255), (byte)_rnd.Next(255), (byte)255);
                        }
                        _txDisplayTexture.Lock();
                        _txDisplayTexture.SetPixels(caPixelPile);
                        _txDisplayTexture.Unlock();

                        c8CHIP8.GraphicsMemoryChanged = false;
                    }
                    if (c8CHIP8.MemoryChanged)
                    {
                        memMap.UpdateTable(c8CHIP8.Memory);
                        c8CHIP8.MemoryChanged = false;
                    }
                    // update the debug window
                    if (dbgWindow.Visible)
                    {
                        dbgWindow.PC = c8CHIP8.PC;
                        dbgWindow.I = c8CHIP8.I;
                        dbgWindow.OpCode = c8CHIP8.OpCode;
                        dbgWindow.Cycle = c8CHIP8.CycleCounter;
                        dbgWindow.SP = c8CHIP8.SP;
                        dbgWindow.SoundTimer = c8CHIP8.SoundTimer;
                        dbgWindow.DelayTimer = c8CHIP8.DelayTimer;
                        dbgWindow.Stack = c8CHIP8.stack.ToArray();
                        dbgWindow.Registers = c8CHIP8.V;
                        dbgWindow.UpdateValues();
                    }

                    if (EmuState == EmulationState.Paused && isRunning)
                    { // for the step
                        isRunning = false;
                    }
                }


                //sdl.FrameRateCapper();

                _sdlSDLWrapper.ClearScreen();
                if (c8CHIP8.ROMLoaded)
                {
                    _sdlSDLWrapper.RenderTexture(_txDisplayTexture, null, new SimpleSDLWrapperDotNET.Rectangle(_iLeft, _iTop, _iWidth, _iHeight));
                    //_sdlSDLWrapper.RenderTexture(_txDisplayTexture, null, new SimpleSDLWrapperDotNET.Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y + msMenuStrip.Height, this.ClientRectangle.Width, this.ClientRectangle.Height - msMenuStrip.Height));
                }
                else
                {
                    _sdlSDLWrapper.RenderTexture(_txLogoTexture, null, new SimpleSDLWrapperDotNET.Rectangle(_iLeft + (_iWidth / 2) - (_txLogoTexture.Width / 2), _iTop + (_iHeight / 2) - (_txLogoTexture.Height / 2), _txLogoTexture.Width /*_iWidth / 3*/, _txLogoTexture.Height /*_iHeight / 3*/));
                }
                _sdlSDLWrapper.Render();
            }
        }

        public void c8CHIP8_onPlaySound(object sender, EventArgs e)
        {
            _sampleDing.Play();
        }

        public void sdl_OnKeyPressedOrReleased(object sender, KeyboardEventArgs e)
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
                    _sampleDing.Play();
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

        private void frmMainForm_Load(object sender, EventArgs e)
        {
            this.ClientSize = new Size(640, 320 + msMenuStrip.Height);
        }

        private void debugWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dbgWindow.Show();
        }

        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dbgConsole.Show();
        }

        private void memoryDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            memMap.Show();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null != dbgConsole && !dbgConsole.IsDisposed && !dbgConsole.Disposing)
            {
                dbgConsole.AddLine(DateTime.Now.Ticks.ToString());
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofdOpenRomFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (ofdOpenRomFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    _loadRom();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void _loadRom()
        {
            c8CHIP8.Init();
            c8CHIP8.LoadRom(ofdOpenRomFileDialog.FileName);
            this.Text = "CHIP-8 Emulator - " + Path.GetFileName(ofdOpenRomFileDialog.FileName);
            isRunning = true;
            EmuState = EmulationState.Running;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isRunning = false;
            EmuState = EmulationState.Paused;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isRunning = false;
            EmuState = EmulationState.Stopped;
            c8CHIP8.Init();
            this.Text = "CHIP-8 Emulator";
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isRunning = true;
            EmuState = EmulationState.Running;
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _loadRom();
        }
    }

}
