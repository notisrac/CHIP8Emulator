using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIP8Library
{
    public class CHIP8
    {
        private const int MEM_FONTSETSTART = 0;
        private const int MEM_PROGRAMSTART = 0x200;
        private const byte SPRITE_WIDTH = 8;
        public const int GRAPHICS_WIDTH = 64;
        public const int GRAPHICS_HEIGHT = 32;
        public const int REGISTERCOUNT = 16;

        private byte[] _baMemory = new byte[4096];
        /// <summary>
        /// CHIP-8 was most commonly implemented on 4K systems, such as the Cosmac VIP and the Telemac 1800. 
        /// These machines had 4096 (0x1000) memory locations, all of which are 8 bits (a byte) which is where 
        /// the term CHIP-8 originated. However, the CHIP-8 interpreter itself occupies the first 512 bytes 
        /// of the memory space on these machines. For this reason, most programs written for the original 
        /// system begin at memory location 512 (0x200) and do not access any of the memory below the location 512 (0x200).
        /// The uppermost 256 bytes (0xF00-0xFFF) are reserved for display refresh, and the 96 bytes below that (0xEA0-0XEFF) 
        /// were reserved for call stack, internal use, and other variables. In modern CHIP-8 implementations, 
        /// there is no need for any of the memory space to be used, but it is common to store font data in the lower 512 bytes (0x000-0x200).
        /// Memory map:
        ///   0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        ///   0x000-0x050 - Used for the built in 4x5 pixel font set (0-F)
        ///   0x200-0xFFF - Program ROM and work RAM
        /// </summary>
        public byte[] Memory
        {
            get { return _baMemory; }
        }

        public bool MemoryChanged { get; set; }

        private byte[] _baRegisters = new byte[REGISTERCOUNT];
        /// <summary>
        /// CHIP-8 has 16 8-bit data registers named from V0 to VF. The VF register doubles as a carry flag. 
        /// The address register, which is named I, is 16 bits wide and is used with several opcodes that involve memory operations.
        /// </summary>
        public byte[] V
        {
            get { return _baRegisters; }
            set { _baRegisters = value; }
        }

        private short _sAddressRegister;
        /// <summary>
        /// The address register, which is named I, is 16 bits wide and is used with several opcodes that involve memory operations.
        /// </summary>
        public short I
        {
            get { return _sAddressRegister; }
            set { _sAddressRegister = value; }
        }

        private short _sProgramCounter;
        /// <summary>
        /// Program counter - the current position in the memory
        /// </summary>
        public short PC
        {
            get { return _sProgramCounter; }
            set { _sProgramCounter = value; }
        }

        private Stack<short> _ssStack = new Stack<short>();
        /// <summary>
        /// The stack is only used to store return addresses when subroutines are called.
        /// The original 1802 version allocated 48 bytes for up to 12 levels of nesting; modern implementations normally have at least 16 levels.
        /// </summary>
        public Stack<short> stack
        {
            get { return _ssStack; }
        }

        /// <summary>
        /// Stack pointer
        /// </summary>
        public int SP
        {
            get { return _ssStack.Count; }
        }

        private int _iDelayTimer;
        /// <summary>
        /// This timer is intended to be used for timing the events of games. Its value can be set and read.
        /// They both count down at 60 hertz, until they reach 0.
        /// </summary>
        public int DelayTimer
        {
            get { return _iDelayTimer; }
            set { _iDelayTimer = value; }
        }

        private int _iSoundTimer;
        /// <summary>
        /// This timer is used for sound effects. When its value is nonzero, a beeping sound is made.
        /// They both count down at 60 hertz, until they reach 0.
        /// </summary>
        public int SoundTimer
        {
            get { return _iSoundTimer; }
            set { _iSoundTimer = value; }
        }

        private ushort _usOpCode;
        /// <summary>
        /// The current operation code
        /// </summary>
        public ushort OpCode
        {
            get { return _usOpCode; }
            set { _usOpCode = value; }
        }

        private byte[] _baGraphicsMemory = new byte[/*64 * 32*/ GRAPHICS_WIDTH * GRAPHICS_HEIGHT];
        /// <summary>
        /// Display resolution is 64×32 pixels, and color is monochrome.
        /// </summary>
        public byte[] GraphicsMemory
        {
            get { return _baGraphicsMemory; }
        }

        /// <summary>
        /// Indicates whether the graphics memory has been changed or not. This should indicate the plotting of the bytes store in the gmem
        /// </summary>
        public bool GraphicsMemoryChanged { get; set; }

        private byte[] _baKeyPad = new byte[16];
        /// <summary>
        /// Input is done with a hex keyboard that has 16 keys which range from 0 to F. The '8', '4', '6', and '2' keys are typically used for directional input.
        /// </summary>
        public byte[] KeyPad
        {
            get { return _baKeyPad; }
        }

        /// <summary>
        /// The fontset
        /// </summary>
        private byte[] _baFontSet = new byte[]{ 
          0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
          0x20, 0x60, 0x20, 0x20, 0x70, // 1
          0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
          0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
          0x90, 0x90, 0xF0, 0x10, 0x10, // 4
          0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
          0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
          0xF0, 0x10, 0x20, 0x40, 0x40, // 7
          0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
          0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
          0xF0, 0x90, 0xF0, 0x90, 0x90, // A
          0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
          0xF0, 0x80, 0x80, 0x80, 0xF0, // C
          0xE0, 0x90, 0x90, 0x90, 0xE0, // D
          0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
          0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };
        public byte[] FontSet
        {
            get { return _baFontSet; }
        }

        /// <summary>
        /// Called whenever a sound needs to be played. In the case of a CHIP-8 this is a single beep.
        /// </summary>
        public event EventHandler onPlaySound;

        public bool ROMLoaded { get; set; }

        /// <summary>
        /// The number of cycles that have been emulated sofar
        /// </summary>
        public ulong CycleCounter { get; private set; }

        private Random _rndRandom = new Random();

        /// <summary>
        /// Called whenever a new debug message is available.
        /// </summary>
        public event EventHandler<DebugEventArgs> onDebugMessage;
        public bool DebugEnabled { get; set; }

        public CHIP8()
        {
        }

        /// <summary>
        /// Initialize this instance of the chip8 emulator
        /// </summary>
        public void Init()
        {
            try
            {
                // reset the registers
                V.ToList().ForEach(r => r = 0);

                // reset the address (index) register
                I = 0;

                // reset the current opcode
                OpCode = 0;

                // set the program counter to 200 - where the loaded program starts
                PC = MEM_PROGRAMSTART;

                // clean up the stack
                stack.Clear();

                // clear the graphics mem
                //GraphicsMemory.ToList().ForEach(gm => gm = 0);
                //_baGraphicsMemory = Enumerable.Repeat((byte)0, GraphicsMemory.Length).ToArray();
                _clearByteArray(ref _baGraphicsMemory);

                // changed the mem, indicate it
                GraphicsMemoryChanged = true;

                // clear the memory
                //Memory.ToList().ForEach(m => m = 0);
                //_baMemory = Enumerable.Repeat((byte)0, Memory.Length).ToArray();
                _clearByteArray(ref _baMemory);
                MemoryChanged = true;

                // reset the key states
                //KeyPad.ToList().ForEach(k => k = 0);
                //_baKeyPad = Enumerable.Repeat((byte)0, KeyPad.Length).ToArray();
                _clearByteArray(ref _baKeyPad);

                // reset the timers
                DelayTimer = 0;
                SoundTimer = 0;

                // load the fontset
                FontSet.CopyTo(Memory, MEM_FONTSETSTART);

                // there is no rom loaded yet
                ROMLoaded = false;

                // reset the cycle counter
                CycleCounter = 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to initialize the emulator", ex);
            }
        }

        /// <summary>
        /// Loads the specified rom into the memory. (starting at 0x200 == 512)
        /// </summary>
        /// <param name="fileName">Full name and path of the rom to load</param>
        public void LoadRom(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException("Rom file \"" + fileName + "\" not found!", fileName);
                }

                // load all the bytes from the file into the temp storage
                byte[] tmpRom = File.ReadAllBytes(fileName);
                // copy the rom into the memory
                tmpRom.CopyTo(Memory, 512);
                ROMLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to load the rom \"" + fileName + "\": " + ex.Message, ex);
            }
        }


        public void EmulateCycle()
        {
            short calcPC = 0;

            // get the current opcode from the memory
            OpCode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            switch (OpCode & 0xF000) // what is the first byte of the opcode?
            {
                case 0x0000:
                    switch (OpCode) // fixed opcode
                    {
                        case 0x00E0: // 00E0: Clears the screen.
                            _debug("CLS");
                            //GraphicsMemory.ToList().ForEach(gm => gm = 0);
                            _clearByteArray(ref _baGraphicsMemory);
                            GraphicsMemoryChanged = true;
                            PC += 2; // next instruction
                            break;
                        case 0x00EE: // 00EE: Returns from a subroutine.
                            _debug("RET");
                            PC = stack.Pop(); // set the program counter to the memory position of the last subroutine call from the top of the stack
                            PC += 2; // next instruction
                            break;
                        case 0x0000: // 0000: ????
                            PC += 2; // next instruction
                            break;
                        default:
                            // invalid opcode
                            ThrowInvalidOpCodeException();
                            break;
                    }
                    break;
                case 0x1000: // 1NNN: Jumps to address NNN.
                    calcPC = (short)(OpCode & 0x0FFF);
                    _debug("JP", val: calcPC);
                    PC = calcPC; // set the program counter to NNN
                    break;
                case 0x2000: // 2NNN: Calls subroutine at NNN.
                    calcPC = (short)(OpCode & 0x0FFF);
                    _debug("CALL", val: calcPC);
                    stack.Push(PC); // store the current location
                    PC = calcPC; // set the program counter to NNN
                    break;
                case 0x3000: // 3XNN: Skips the next instruction if VX equals NN.
                    _debug("SE", getValueAt(0x0F00), val: (OpCode & 0x00FF));
                    PC = (short)(((OpCode & 0x00FF) == V[getValueAt(0x0F00)]) ? PC + 4 : PC + 2);
                    break;
                case 0x4000: // 4XNN: Skips the next instruction if VX doesn't equal NN.
                    _debug("SNE", getValueAt(0x0F00), val: (OpCode & 0x00FF));
                    PC = (short)(((OpCode & 0x00FF) != V[getValueAt(0x0F00)]) ? PC + 4 : PC + 2);
                    break;
                case 0x5000: // 5XY0: Skips the next instruction if VX equals VY.
                    _debug("SE", getValueAt(0x0F00), getValueAt(0x00F0));
                    PC = (short)((V[getValueAt(0x00F0)] == V[getValueAt(0x0F00)]) ? PC + 4 : PC + 2);
                    break;
                case 0x6000: // 6XNN: Sets VX to NN.
                    _debug("LD", getValueAt(0x0F00), val: (byte)(OpCode & 0x00FF));
                    V[getValueAt(0x0F00)] = (byte)(OpCode & 0x00FF);
                    PC += 2; // next instruction
                    break;
                case 0x7000: // 7XNN: Adds NN to VX.
                    _debug("ADD", getValueAt(0x0F00), val: (byte)(OpCode & 0x00FF));
                    V[getValueAt(0x0F00)] += (byte)(OpCode & 0x00FF);
                    PC += 2; // next instruction
                    break;
                case 0x8000: // 8xxx opcodes
                    switch (OpCode & 0x000F) // check the last 4 bits
                    {
                        case 0x0000: // 8XY0: Sets VX to the value of VY.
                            _debug("LD", getValueAt(0x0F00), getValueAt(0x00F0));
                            V[getValueAt(0x0F00)] = V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0001: // 8XY1: Sets VX to VX or VY.
                            _debug("OR", getValueAt(0x0F00), getValueAt(0x00F0));
                            V[getValueAt(0x0F00)] |= V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0002: // 8XY2: Sets VX to VX and VY.
                            _debug("AND", getValueAt(0x0F00), getValueAt(0x00F0));
                            V[getValueAt(0x0F00)] &= V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0003: // 8XY3: Sets VX to VX xor VY.
                            _debug("XOR", getValueAt(0x0F00), getValueAt(0x00F0));
                            V[getValueAt(0x0F00)] ^= V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0004: // 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                            _debug("ADD", getValueAt(0x0F00), getValueAt(0x00F0));
                            if (V[getValueAt(0x00F0)] > (0xFF - V[getValueAt(0x0F00)]))
                            {
                                V[0xF] = 1; // carry
                            }
                            else
                            {
                                V[0xF] = 0;
                            }
                            V[getValueAt(0x0F00)] += V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0005: // 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            _debug("SUB", getValueAt(0x0F00), getValueAt(0x00F0));
                            if (V[getValueAt(0x00F0)] > V[getValueAt(0x0F00)])
                            {
                                V[0xF] = 0; // borrow
                            }
                            else
                            {
                                V[0xF] = 1;
                            }
                            V[getValueAt(0x0F00)] -= V[getValueAt(0x00F0)];
                            PC += 2; // next instruction
                            break;
                        case 0x0006: // 8XY6: Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
                            _debug("SHR", getValueAt(0x0F00));
                            V[0xF] = (byte)(V[getValueAt(0x0F00)] & 0x1);
                            V[getValueAt(0x0F00)] >>= 1;
                            PC += 2; // next instruction
                            break;
                        case 0x0007: // 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            _debug("SUBN", getValueAt(0x0F00), getValueAt(0x00F0));
                            if (V[getValueAt(0x0F00)] > V[getValueAt(0x00F0)])
                            {
                                V[0xF] = 0; // borrow
                            }
                            else
                            {
                                V[0xF] = 1;
                            }
                            V[getValueAt(0x0F00)] = (byte)(V[getValueAt(0x00F0)] - V[getValueAt(0x0F00)]);
                            PC += 2; // next instruction
                            break;
                        case 0x000E: // 8XYE: Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
                            _debug("SHL", getValueAt(0x0F00));
                            V[0xF] = (byte)(V[getValueAt(0x0F00)] >> 7);
                            V[getValueAt(0x0F00)] <<= 1;
                            PC += 2; // next instruction
                            break;
                        default:
                            // unknown opcode
                            ThrowInvalidOpCodeException();
                            break;
                    }
                    break;
                case 0x9000: // 9XY0: Skips the next instruction if VX doesn't equal VY.
                    _debug("SNE", getValueAt(0x0F00), getValueAt(0x00F0));
                    if (V[getValueAt(0x0F00)] != V[getValueAt(0x00F0)])
                    {
                        PC += 4; // next next instruction
                    }
                    else
                    {
                        PC += 2; // next instruction
                    }
                    break;
                case 0xA000: // ANNN: Sets I to the address NNN.
                    _debug("LD", (int)SpecialRegisters.I, val: (OpCode & 0x0FFF));
                    I = (short)(OpCode & 0x0FFF);
                    PC += 2; // next instruction
                    break;
                case 0xB000: // BNNN: Jumps to the address NNN plus V0.
                    _debug("JP", 0, val: (OpCode & 0x0FFF));
                    PC = (short)((OpCode & 0x0FFF) + V[0x0]);
                    break;
                case 0xC000: // CXNN: Sets VX to a random number and NN.
                    _debug("RND", getValueAt(0x0F00), val: (OpCode & 0x00FF));
                    V[getValueAt(0x0F00)] = (byte)((_rndRandom.Next() % 0xFF) & (OpCode & 0x00FF)); // TODO check the random value
                    PC += 2; // next instruction
                    break;
                case 0xD000: // DXYN: Sprites stored in memory at location in index register (I), maximum 8bits wide.
                    _debug("DRW", getValueAt(0x0F00), getValueAt(0x00F0), getValueAt(0x000F));
                    // Wraps around the screen. If when drawn, clears a pixel, register VF is set to 1 otherwise it is zero. All drawing is XOR drawing (e.g. it toggles the screen pixels)
                    // Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                    // Each row of 8 pixels is read as bit-coded starting from memory location I; 
                    // I value doesn't change after the execution of this instruction. 
                    // VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, 
                    // and to 0 if that doesn't happen

                    V[0xF] = 0; // initial state is that there is no overwriting occured
                    byte spriteXPos = V[getValueAt(0x0F00)];
                    byte spriteYPos = V[getValueAt(0x00F0)];
                    byte spriteHeight = getValueAt(0x000F);

                    for (int spriteY = 0; spriteY < spriteHeight; spriteY++)
                    {
                        ushort onePixel = Memory[I + spriteY];
                        for (int spriteX = 0; spriteX < SPRITE_WIDTH; spriteX++)
                        {
                            if ((onePixel & (0x80 >> spriteX)) != 0)
                            {
                                int pos = spriteXPos + spriteX + ((spriteYPos + spriteY) * 64);
                                if (pos < GraphicsMemory.Length)
                                {
                                    if (GraphicsMemory[pos] == 1)
                                    {
                                        V[0xF] = 1;
                                    }
                                    GraphicsMemory[pos] ^= 1;
                                }
                                else
                                { // overflow!
                                    // TODO display error
                                }
                            }
                        }

                    }
                    GraphicsMemoryChanged = true;

                    PC += 2; // next instruction
                    break;
                case 0xE000: // the keypad opcodes
                    switch (OpCode & 0x00FF)
                    {
                        case 0x009E: // EX9E: Skips the next instruction if the key stored in VX is pressed.
                            _debug("SKP", getValueAt(0x0F00));
                            if (KeyPad[V[getValueAt(0x0F00)]] != 0)
                            {
                                PC += 4; // next next instruction
                            }
                            else
                            {
                                PC += 2; // next instruction
                            }
                            break;
                        case 0x00A1: // EXA1: Skips the next instruction if the key stored in VX isn't pressed.
                            _debug("SKNP", getValueAt(0x0F00));
                            if (KeyPad[V[getValueAt(0x0F00)]] == 0)
                            {
                                PC += 4; // next next instruction
                            }
                            else
                            {
                                PC += 2; // next instruction
                            }
                            break;
                        default:
                            // unknown opcode
                            ThrowInvalidOpCodeException();
                            break;
                    }
                    break;
                case 0xF000: // F series
                    switch (OpCode & 0x00FF)
                    {
                        case 0x0007: // FX07: Sets VX to the value of the delay timer.
                            _debug("LD", getValueAt(0x0F00), (int)SpecialRegisters.DT);
                            V[getValueAt(0x0F00)] = (byte)DelayTimer;
                            PC += 2; // next instruction
                            break;
                        case 0x000A: // FX0A: A key press is awaited, and then stored in VX.
                            _debug("LD", getValueAt(0x0F00), (int)SpecialRegisters.K);
                            bool foundKey = false;
                            for (byte i = 0; i < KeyPad.Length; i++)
                            {
                                if (KeyPad[i] != 0)
                                {
                                    V[getValueAt(0x0F00)] = i;
                                    foundKey = true;
                                    break;
                                }
                            }
                            if (foundKey)
                            {
                                PC += 2; // next instruction
                            }
                            else
                            { // no keypress was detected in this cycle, keep waiting
                                return;
                            }
                            break;
                        case 0x0015: // FX15: Sets the delay timer to VX.
                            _debug("LD", (int)SpecialRegisters.DT, getValueAt(0x0F00));
                            DelayTimer = V[getValueAt(0x0F00)];
                            PC += 2; // next instruction
                            break;
                        case 0x0018: // FX18: Sets the sound timer to VX.
                            _debug("LD", (int)SpecialRegisters.ST, getValueAt(0x0F00));
                            SoundTimer = V[getValueAt(0x0F00)];
                            PC += 2; // next instruction
                            break;
                        case 0x001E: // FX1E: Adds VX to I. VF is set to 1 when range overflow (I+VX>0xFFF), and 0 when there isn't. This is undocumented feature of the Chip-8 and used by Spacefight 2019! game.
                            _debug("ADD", (int)SpecialRegisters.I, getValueAt(0x0F00));
                            if ((I + V[getValueAt(0x0F00)]) > 0xFFF)
                            {
                                V[0xF] = 1;
                            }
                            else
                            {
                                V[0xF] = 0;
                            }
                            I += V[getValueAt(0x0F00)];
                            PC += 2; // next instruction
                            break;
                        case 0x0029: // FX29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            _debug("LD", (int)SpecialRegisters.F, getValueAt(0x0F00));
                            I = (short)(MEM_FONTSETSTART + V[getValueAt(0x0F00)] * 5); // 5bytes per char
                            PC += 2; // next instruction
                            break;
                        case 0x0033: // FX33: Stores the Binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
                            _debug("LD", (int)SpecialRegisters.B, getValueAt(0x0F00));
                            // (In other words, take the decimal representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.)
                            byte registerValue = V[getValueAt(0x0F00)];
                            Memory[I] = (byte)(registerValue / 100);
                            Memory[I + 1] = (byte)((registerValue / 10) % 10);
                            Memory[I + 2] = (byte)((registerValue % 100) % 10);
                            MemoryChanged = true;
                            PC += 2; // next instruction
                            break;
                        case 0x0055: // FX55: Stores V0 to VX in memory starting at address I.
                            _debug("LD", (int)SpecialRegisters.IAddr, getValueAt(0x0F00));
                            V.Take(V[getValueAt(0x0F00)] + 1).ToList().CopyTo(Memory, I);
                            // On the original interpreter, when the operation is done, I=I+X+1. On current implementations, I is left unchanged.
                            I += (short)(V[getValueAt(0x0F00)] + 1); // TODO should this be ignored?
                            PC += 2; // next instruction
                            break;
                        case 0x0065: // FX65: Fills V0 to VX with values from memory starting at address I.
                            _debug("LD", getValueAt(0x0F00), (int)SpecialRegisters.IAddr);
                            for (int i = 0; i <= getValueAt(0x0F00); i++)
                            {
                                V[i] = Memory[I + i];
                            }
                            // On the original interpreter, when the operation is done, I=I+X+1. On current implementations, I is left unchanged.
                            I += (short)(V[getValueAt(0x0F00)] + 1); // TODO should this be ignored?
                            PC += 2; // next instruction
                            break;
                        default:
                            // unknown opcode
                            ThrowInvalidOpCodeException();
                            break;
                    }
                    break;
                default:
                    // unknown opcode
                    ThrowInvalidOpCodeException();
                    break;
            }

            // decrement the delay timer
            if (DelayTimer > 0)
            {
                DelayTimer--;
            }
            // decrement the sound timer
            if (SoundTimer > 0)
            {
                if (1 == SoundTimer)
                {
                    // beep
                    OnPlaySound(EventArgs.Empty);
                }
                SoundTimer--;
            }

            CycleCounter++;
        }

        private void ThrowInvalidOpCodeException()
        {
            //throw new Exception(string.Format("Invalid opcode! \"0x{0:X4}\"", OpCode));
            _debug("ERROR", message: "Invalid opCode!");
        }

        protected virtual void OnPlaySound(EventArgs e)
        {
            if (null != onPlaySound)
            {
                onPlaySound(this, e);
            }
        }

        private void _debug(string operation, int regX = -1, int regY = -1, int val = -1, string message = "")
        {
            if (DebugEnabled)
            {
                string sDebugMessage = string.Format("{0:X4}: {1:X4}: {2,-5}", PC, OpCode, operation);
                if (regX > -1)
                { // there is a register X
                    if (regX >= 100)
                    { // this is a special register
                        SpecialRegisters sr = (SpecialRegisters)regX;
                        sDebugMessage += (((sr == SpecialRegisters.IAddr) ? "[I]" : sr.ToString()) + ",").PadRight(5, ' ');
                    }
                    else
                    {
                        sDebugMessage += string.Format("V{0:X},", regX).PadRight(5, ' ');
                    }
                }
                if (regY > -1)
                { // there is a register Y
                    if (regY >= 100)
                    { // this is a special register
                        SpecialRegisters sr = (SpecialRegisters)regY;
                        sDebugMessage += (((sr == SpecialRegisters.IAddr) ? "[I]" : sr.ToString()) + ",").PadRight(5, ' ');
                    }
                    else
                    {
                        sDebugMessage += string.Format("V{0:X},", regY).PadRight(5, ' ');
                    }
                }
                if (val > -1)
                { // there is a value
                    sDebugMessage += string.Format("#{0:X}", val).PadRight(5, ' ');
                }
                sDebugMessage = sDebugMessage.TrimEnd().TrimEnd(new char[] { ',' });
                if (!string.IsNullOrEmpty(message))
                {
                    sDebugMessage += "  ;" + message;
                }

                OnDebugMessage(new DebugEventArgs(sDebugMessage));
            }
        }

        protected virtual void OnDebugMessage(DebugEventArgs e)
        {
            if (null != onDebugMessage)
            {
                onDebugMessage(this, e);
            }
        }

        private byte getValueAt(int pos)
        {
            switch (pos)
            {
                case 0x000F:
                    return (byte)(OpCode & pos);
                    break;
                case 0x00F0:
                    return (byte)((OpCode & pos) >> 4);
                    break;
                case 0x0F00:
                    return (byte)((OpCode & pos) >> 8);
                    break;
                default:
                    return (byte)(OpCode & pos);
                    break;
            }
        }

        private void _clearByteArray(ref byte[] target)
        {
            //target = Enumerable.Repeat((byte)0, target.Length).ToArray();
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = 0;
            }
        }
    }

    public enum SpecialRegisters
    {
        I = 100,
        DT = 101,
        K = 102,
        ST = 103,
        F = 104,
        B = 105,
        IAddr = 106
    }

    public class DebugEventArgs : EventArgs
    {
        public string Message { get; set; }

        public DebugEventArgs(string message)
        {
            Message = message;
        }
    }
}
