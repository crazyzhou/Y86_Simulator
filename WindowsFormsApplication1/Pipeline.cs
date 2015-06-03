using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipeline_Simulator_by_Gu_and_Zhou
{
    public class Pipeline
    {
        #region variable
        public Dictionary<int, string> Dic_instr = new Dictionary<int, string>(); //指令字典
        public Dictionary<int, long> Dic_mem = new Dictionary<int, long>(); //内存字典（内存存储简化版，4byte）
        public Dictionary<Register, int> Dic_regids = new Dictionary<Register, int>(); //寄存器字典
        public Dictionary<int, byte> Dic_memory = new Dictionary<int, byte>(); //内存字典（真实内存，1byte）

        public const int maxNum = 1000;
        public const int maxMemory = 2048;

        //每个cycle的每个stage的寄存器保存
        public int[] fetch_pcsave = new int[maxNum], decode_pcsave = new int[maxNum], execute_pcsave = new int[maxNum];
        public int[] memory_pcsave = new int[maxNum], writeback_pcsave = new int[maxNum];
        public int[] eaxsave = new int[maxNum], ecxsave = new int[maxNum];
        public int[] edxsave = new int[maxNum], ebxsave = new int[maxNum];
        public int[] espsave = new int[maxNum], ebpsave = new int[maxNum];
        public int[] esisave = new int[maxNum], edisave = new int[maxNum];
        public int[] F_predPCsave=new int[maxNum],  D_ifunsave=new int[maxNum],  E_ifunsave = new int[maxNum];
        public int[] D_valCsave=new int[maxNum],  D_valPsave=new int[maxNum],  E_valAsave=new int[maxNum],  E_valBsave=new int[maxNum],  E_valCsave = new int[maxNum];
        public int[] M_valAsave=new int[maxNum],  M_valEsave=new int[maxNum],  W_valEsave=new int[maxNum],  W_valMsave = new int[maxNum];
        public bool[] ZFsave = new bool[maxNum], SFsave = new bool[maxNum], OFsave = new bool[maxNum];
        public Instruction[] D_icodesave=new Instruction[maxNum],  E_icodesave=new Instruction[maxNum],  M_icodesave=new Instruction[maxNum],  W_icodesave = new Instruction[maxNum];
        public Register[] D_rAsave=new Register[maxNum],  D_rBsave = new Register[maxNum];
        public Register[] E_dstEsave=new Register[maxNum],  M_dstEsave=new Register[maxNum],  W_dstEsave = new Register[maxNum];
        public Register[] E_dstMsave=new Register[maxNum],  M_dstMsave=new Register[maxNum],  W_dstMsave = new Register[maxNum];
        public Register[] E_srcAsave=new Register[maxNum],  E_srcBsave = new Register[maxNum];
        public int[] f_stsave = new int[maxNum], d_stsave = new int[maxNum];
        public int[] e_stsave = new int[maxNum], m_stsave = new int[maxNum];
        public long[,] memorySave=new long[maxMemory,maxNum];
        public long[,] memSave = new long[maxMemory, maxNum];

        public bool ZF, SF, OF;
        public Status D_stat, E_stat, M_stat, W_stat, m_stat, f_stat, d_stat, e_stat;
        public Instruction D_icode, E_icode, M_icode, W_icode, f_icode, e_icode, d_icode, m_icode;
        public int D_ifun, E_ifun, f_ifun, e_alufun, d_ifun, e_ifun;
        public Register f_rA, f_rB, D_rA, D_rB;
        public int E_valA, M_valA, d_valA, e_valA, m_valA;
        public int E_valB, d_valB, e_valB;
        public int D_valC, f_valC, E_valC, d_valC, e_valC;
        public int M_valE, W_valE, e_valE, m_valE;
        public int D_valP, f_valP;
        public int W_valM, m_valM;
        public Register E_dstE, M_dstE, W_dstE, d_dstE, e_dstE, m_dstE;
        public Register E_dstM, M_dstM, W_dstM, d_dstM, e_dstM, m_dstM;
        public Register E_srcA, d_srcA;
        public Register E_srcB, d_srcB;
        public int F_predPC, f_prePC;
        public bool M_Bch, e_Bch;
        public int d_rvalA, d_rvalB;
        public int m_addr;
        public bool i_mem_error, mem_error;
        public int cycle;
        public int fetch_pc, decode_pc, execute_pc, memory_pc, writeback_pc; //pc传递变量，为了得到即将处理的指令
        public int f_st, d_st, e_st, m_st; //bubble or stall or normal
        #endregion

        public void get_instruction(string filename)
        {
            FileStream aFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(aFile);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            string line = sr.ReadLine();
            while (line != null)
            {
                int i = line.IndexOf('|');
                int j = line.IndexOf(':');
                if (j == -1)
                {
                    line = sr.ReadLine();
                    continue;
                }
                string addr_s = line.Substring(0, j).Trim();
                string instr_s = line.Substring(j + 1, i - j - 1).Trim();
                if (instr_s == "")
                {
                    line = sr.ReadLine();
                    continue;
                }
                int addr_i = Convert.ToInt32(addr_s, 16);
                if (instr_s.Length == 8) //内存
                {
                    string mem_content = instr_s.Substring(6, 2) + instr_s.Substring(4, 2) + instr_s.Substring(2, 2) + instr_s.Substring(0, 2);
                    long mem_i = Convert.ToInt64(mem_content, 16);
                    // 添加简化版memory
                    Dic_mem.Add(addr_i, mem_i);
                    // 添加真实版memory
                    Dic_memory.Add(addr_i,Convert.ToByte(Convert.ToInt16(instr_s.Substring(0, 2),16)));
                    Dic_memory.Add(addr_i+1,Convert.ToByte(Convert.ToInt16(instr_s.Substring(2, 2),16)));
                    Dic_memory.Add(addr_i+2,Convert.ToByte(Convert.ToInt16(instr_s.Substring(4, 2),16)));
                    Dic_memory.Add(addr_i+3,Convert.ToByte(Convert.ToInt16(instr_s.Substring(6, 2),16)));
                }
                else   //指令
                {
                    //添加到指令字典
                    Dic_instr.Add(addr_i, instr_s);
                    //添加真实版memory
                    int l = Dic_instr[addr_i].Length;
                    int tmp = 0;
                    int tmp1 = 0;
                    while (tmp < l)
                    {
                        Dic_memory.Add(addr_i + tmp1, Convert.ToByte(Convert.ToInt16(Dic_instr[addr_i].Substring(tmp, 2),16)));
                        tmp1++;
                        tmp += 2;
                    }
                }  
                line = sr.ReadLine();
            }
            //cycle0的内存保存
            for (int i = 0; i < maxMemory; i++)
                if (Dic_memory.ContainsKey(i))
                    memorySave[i, 0] = Dic_memory[i];
            for (int i = 0; i < maxMemory; i++)
                if (Dic_mem.ContainsKey(i))
                    memSave[i, 0] = Dic_mem[i];
        }

        #region reset
        public void reset()
        {
            fetch_pcsave[0] = 0;
            decode_pcsave[0] = execute_pcsave[0] = memory_pcsave[0] = writeback_pcsave[0] = -1;
            fetch_pc = 0;
            decode_pc = execute_pc = memory_pc = writeback_pc = -1;
            f_st = d_st = e_st = m_st = 0;
            D_stat = E_stat = M_stat = W_stat = m_stat = f_stat = d_stat = e_stat = Status.AOK;
            D_icode = E_icode = M_icode = W_icode = f_icode = d_icode = e_icode = m_icode = 0;
            D_ifun = E_ifun = f_ifun = e_alufun = d_ifun = e_ifun = 0;
            f_rA = f_rB = D_rA = D_rB = Register.none;
            E_valA = M_valA = d_valA = e_valA = m_valA = 0;
            E_valB = d_valB = e_valB = 0;
            D_valC = f_valC = E_valC = d_valC = e_valC = 0;
            M_valE = W_valE = e_valE = m_valE = 0;
            D_valP = f_valP = 0;
            W_valM = m_valM = 0;
            E_dstE = M_dstE = W_dstE = d_dstE = e_dstE = m_dstE = Register.none;
            E_dstM = M_dstM = W_dstM = d_dstM = e_dstM = m_dstM = Register.none;
            E_srcA = d_srcA = Register.none;
            E_srcB = d_srcB = Register.none;
            F_predPC = f_prePC = 0;
            M_Bch = e_Bch = false;
            d_rvalA = d_rvalB = 0;
            m_addr = 0;
            i_mem_error = mem_error = false;
            SF = ZF = OF = false;
            cycle = 0;
            Dic_regids.Clear();
            Dic_regids.Add(Register.eax, 0);
            Dic_regids.Add(Register.ecx, 0);
            Dic_regids.Add(Register.edx, 0);
            Dic_regids.Add(Register.ebx, 0);
            Dic_regids.Add(Register.esp, 0);
            Dic_regids.Add(Register.ebp, 0);
            Dic_regids.Add(Register.esi, 0);
            Dic_regids.Add(Register.edi, 0);
            Dic_regids.Add(Register.none, 0);
            Dic_memory.Clear();
            for (int i = 0; i < maxMemory; i++)
                if (memorySave[i, 0] != 0)
                    Dic_memory.Add(i, (byte) memorySave[i, 0]);
            Dic_mem.Clear();
            for (int i = 0; i < maxMemory; i++)
                if (memSave[i, 0] != 0)
                    Dic_mem.Add(i, memSave[i, 0]);
            return;
        }
        #endregion

        #region Fetch
        public int f_pc()
        {
            if (M_icode == Instruction.jXX && !M_Bch)
                return M_valA;
            if (W_icode == Instruction.ret)
                return W_valM;
            return F_predPC;
        }
        public bool need_regids()
        {
            Instruction[] tmp = { Instruction.rrmovl, Instruction.OPl, Instruction.pushl, Instruction.popl, Instruction.irmovl, Instruction.rmmovl, Instruction.mrmovl };
            if (tmp.Contains(f_icode))
                return true;
            return false;
        }
        public bool need_valC()
        {
            Instruction[] tmp = { Instruction.irmovl, Instruction.rmmovl, Instruction.mrmovl, Instruction.jXX, Instruction.call };
            if (tmp.Contains(f_icode))
                return true;
            return false;
        }
        public bool instr_valid()
        {
            Instruction[] tmp = { Instruction.nop, Instruction.halt, Instruction.rrmovl, Instruction.irmovl, Instruction.rmmovl, Instruction.mrmovl, Instruction.OPl, Instruction.jXX, Instruction.call, Instruction.ret, Instruction.pushl, Instruction.popl };
            if (tmp.Contains(f_icode))
                return true;
            return false;
        }
        public int new_F_predPC()
        {
            Instruction[] tmp = { Instruction.jXX, Instruction.call };
            if (tmp.Contains(f_icode))
                return f_valC;
            return f_valP;
        }

        public void Fetch()
        {
            string instr;
            try
            {
                instr = Dic_instr[f_pc()];
            }
            catch
            {
                Dic_instr.Add(f_pc(), "00");
                instr = Dic_instr[f_pc()];
            }
            f_icode = (Instruction)Convert.ToInt32(instr.Substring(0, 1), 16);
            f_ifun = Convert.ToInt32(instr.Substring(1, 1), 16);
            if (need_regids())
            {
                f_rA = (Register)Convert.ToInt32(instr.Substring(2, 1), 16);
                f_rB = (Register)Convert.ToInt32(instr.Substring(3, 1), 16);
            }
            else
            {
                f_rA = Register.none;
                f_rB = Register.none;
            }
            if (need_valC())
            {
                if (need_regids())
                {
                    string valC_s = instr.Substring(10, 2) + instr.Substring(8, 2) + instr.Substring(6, 2) + instr.Substring(4, 2);
                    f_valC = Convert.ToInt32(valC_s, 16);
                }
                else
                {
                    string valC_s = instr.Substring(8, 2) + instr.Substring(6, 2) + instr.Substring(4, 2) + instr.Substring(2, 2);
                    f_valC = Convert.ToInt32(valC_s, 16);
                }
            }
            else
            {
                f_valC = 0;
            }
            f_valP = f_pc() + 1;
            if (need_regids())
                f_valP++;
            if (need_valC())
                f_valP += 4;
        }
        #endregion

        #region Decode
        public int new_E_srcA()
        {
            Instruction[] tmp1 = { Instruction.rrmovl, Instruction.rmmovl, Instruction.OPl, Instruction.pushl };
            Instruction[] tmp2 = { Instruction.popl, Instruction.ret };
            if (tmp1.Contains(D_icode))
                return (int)D_rA;
            if (tmp2.Contains(D_icode))
                return (int)Register.esp;
            return (int)Register.none;
        }
        public int new_E_srcB()
        {
            Instruction[] tmp1 = { Instruction.OPl, Instruction.rmmovl, Instruction.mrmovl };
            Instruction[] tmp2 = { Instruction.pushl, Instruction.popl, Instruction.call, Instruction.ret };
            if (tmp1.Contains(D_icode))
                return (int)D_rB;
            if (tmp2.Contains(D_icode))
                return (int)Register.esp;
            return (int)Register.none;
        }
        public int new_E_dstE()
        {
            Instruction[] tmp1 = { Instruction.OPl, Instruction.rrmovl, Instruction.irmovl };
            Instruction[] tmp2 = { Instruction.pushl, Instruction.popl, Instruction.call, Instruction.ret };
            if (tmp1.Contains(D_icode))
                return (int)D_rB;
            if (tmp2.Contains(D_icode))
                return (int)Register.esp;
            return (int)Register.none;
        }
        public int new_E_dstM()
        {
            Instruction[] tmp = { Instruction.mrmovl, Instruction.popl };
            if (tmp.Contains(D_icode))
                return (int)D_rA;
            return (int)Register.none;
        }
        public int new_E_valA()
        {
            Instruction[] tmp = { Instruction.jXX, Instruction.call };
            if (tmp.Contains(D_icode))
                return (int)D_valP;
            if (d_srcA == E_dstE && d_srcA != Register.none)
                return e_valE;
            if (d_srcA == M_dstM && d_srcA != Register.none)
                return m_valM;
            if (d_srcA == M_dstE && d_srcA != Register.none)
                return M_valE;
            if (d_srcA == W_dstM && d_srcA != Register.none)
                return W_valM;
            if (d_srcA == W_dstE && d_srcA != Register.none)
                return W_valE;
            return d_rvalA;
        }
        public int new_E_valB()
        {
            if (d_srcB == E_dstE && d_srcB != Register.none)
                return e_valE;
            if (d_srcB == M_dstM && d_srcB != Register.none)
                return m_valM;
            if (d_srcB == M_dstE && d_srcB != Register.none)
                return M_valE;
            if (d_srcB == W_dstM && d_srcB != Register.none)
                return W_valM;
            if (d_srcB == W_dstE && d_srcB != Register.none)
                return W_valE;
            return d_rvalB;
        }
        public void Decode()
        {
            d_icode = D_icode;
            d_ifun = D_ifun;
            d_dstE = (Register)new_E_dstE();
            d_dstM = (Register)new_E_dstM();
            d_valC = D_valC;
            d_srcA = (Register)new_E_srcA();
            d_srcB = (Register)new_E_srcB();
            d_rvalA = Dic_regids[d_srcA];
            d_rvalB = Dic_regids[d_srcB];
            d_valA = new_E_valA();
            d_valB = new_E_valB();
        }
        #endregion

        #region Execute
        public int aluA()
        {
            if (E_icode == Instruction.rrmovl || E_icode == Instruction.OPl)
                return E_valA;
            if (E_icode == Instruction.irmovl || E_icode == Instruction.rmmovl || E_icode == Instruction.mrmovl)
                return E_valC;
            if (E_icode == Instruction.call || E_icode == Instruction.pushl)
                return -4;
            if (E_icode == Instruction.ret || E_icode == Instruction.popl)
                return 4;
            return 0;
        }
        public int aluB()
        {
            Instruction[] tmp = { Instruction.rmmovl, Instruction.mrmovl, Instruction.OPl, Instruction.call, Instruction.pushl, Instruction.ret, Instruction.popl };
            if (tmp.Contains(E_icode))
                return E_valB;
            if (E_icode == Instruction.rrmovl || E_icode == Instruction.irmovl)
                return 0;
            return 0;
        }
        public int alufun()
        {
            if (E_icode == Instruction.OPl)
                return E_ifun;
            return (int)OPl.addl;
        }
        public bool set_cc()
        {
            if (E_icode == Instruction.OPl)
                return true;
            return false;
        }
        public void Execute()
        {
            e_icode = E_icode;
            e_dstE = E_dstE;
            e_dstM = E_dstM;
            e_valA = E_valA;
            if (alufun() == (int)OPl.addl)
                e_valE = aluA() + aluB();
            else if (alufun() == (int)OPl.andl)
                e_valE = aluA() & aluB();
            else if (alufun() == (int)OPl.subl)
                e_valE = aluB() - aluA();
            else
                e_valE = aluB() ^ aluA();
            if (set_cc())
            {
                OF = ((aluA() < 0 == aluB() < 0) && (e_valE < 0 != aluA() < 0) && alufun() == 0) || ((aluB() < 0 == (~aluA() + 1) < 0) && (e_valE < 0 != aluB() < 0) && alufun() == 1);
                ZF = (e_valE == 0);
                SF = (e_valE < 0);
            }
            //e_Bch
            if (e_icode == Instruction.jXX)
            {
                switch (E_ifun)
                {
                    case 0:
                        e_Bch = true;
                        break;
                    case 1:
                        if ((ZF || SF) && !OF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                    case 2:
                        if (SF && !ZF && !OF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                    case 3:
                        if (ZF && !SF && !OF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                    case 4:
                        if (!ZF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                    case 5:
                        if ((ZF || !SF) && !OF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                    case 6:
                        if (!ZF && !SF && !OF)
                            e_Bch = true;
                        else
                            e_Bch = false;
                        break;
                }
            }
            else
                e_Bch = false;
        }
        #endregion

        #region Memory
        public int mem_addr()
        {
            Instruction[] tmp1 = { Instruction.rmmovl, Instruction.pushl, Instruction.call, Instruction.mrmovl };
            Instruction[] tmp2 = { Instruction.popl, Instruction.ret };
            if (tmp1.Contains(M_icode))
                return M_valE;
            if (tmp2.Contains(M_icode))
                return M_valA;
            return 0;
        }
        public bool mem_read()
        {
            Instruction[] tmp = { Instruction.mrmovl, Instruction.popl, Instruction.ret };
            if (tmp.Contains(M_icode))
                return true;
            return false;
        }
        public bool mem_write()
        {
            Instruction[] tmp = { Instruction.rmmovl, Instruction.pushl, Instruction.call };
            if (tmp.Contains(M_icode))
                return true;
            return false;
        }
        public void Memory()
        {
            m_icode = M_icode;
            m_valE = M_valE;
            m_dstE = M_dstE;
            m_dstM = M_dstM;
            if (mem_read())
            {
                if (mem_addr() < maxMemory)
                    m_valM = (int)Dic_mem[mem_addr()];
                else
                    m_stat = Status.ADR;
            }
            if (mem_write())
            {
                if (mem_addr() < maxMemory){
                    Dic_mem[mem_addr()] = M_valA;
                    Dic_memory[mem_addr()] = (byte)(M_valA & 0xFF);
                    Dic_memory[mem_addr() + 1] = (byte)((M_valA >> 8) & 0xFF);
                    Dic_memory[mem_addr() + 2] = (byte)((M_valA >> 16) & 0xFF);
                    Dic_memory[mem_addr() + 3] = (byte)((M_valA >> 24) & 0xFF);
                }
                else
                    m_stat = Status.ADR;
            }
        }
        #endregion

        #region Write_back
        public void Write_Back()
        {
            if (W_dstM != Register.none)
            {
                Dic_regids[(Register)W_dstM] = W_valM;
            }
            if (W_dstE != Register.none)
            {
                Dic_regids[(Register)W_dstE] = W_valE;
            }
        }
        #endregion

        #region Pipeline_Register_Control
        public bool F_bubble = false;
        public bool F_stall()
        {
            Instruction[] tmp1 = { Instruction.mrmovl, Instruction.popl };
            Register[] tmp2 = { d_srcA, d_srcB };
            Instruction[] tmp3 = { D_icode, E_icode, M_icode };
            return tmp1.Contains(E_icode) && tmp2.Contains(E_dstM) || tmp3.Contains(Instruction.ret);
        }
        public bool D_stall()
        {
            Instruction[] tmp1 = { Instruction.mrmovl, Instruction.popl };
            Register[] tmp2 = { d_srcA, d_srcB };
            return tmp1.Contains(E_icode) && tmp2.Contains(E_dstM);
        }
        public bool D_bubble()
        {
            Instruction[] tmp = { D_icode, E_icode, M_icode };
            return tmp.Contains(Instruction.ret) || (E_icode == Instruction.jXX && !e_Bch);
        }
        public bool E_stall = false;
        public bool E_bubble()
        {
            Instruction[] tmp1 = { Instruction.mrmovl, Instruction.popl };
            Register[] tmp2 = { d_srcA, d_srcB };
            return (E_icode == Instruction.jXX && !e_Bch) || tmp1.Contains(E_icode) && tmp2.Contains(E_dstM);
        }
        public bool M_stall = false;
        public bool M_bubble = false;
        #endregion

        public void PCupdate()
        {
            writeback_pc = memory_pc;
            if (m_st==0)
            {
                memory_pc = execute_pc;
            }
            if (e_st==0)
            {
                execute_pc = decode_pc;
            }
            else execute_pc = -1;
            if (d_st==0)
            {
                decode_pc = fetch_pc;
            }
            else if (d_st==1) decode_pc = -1;
            fetch_pc = f_pc();
        }

        #region Clock
        public void Clock()
        {
            if (!F_stall())
            {
                F_predPC = new_F_predPC();
                f_st = 0;
            }
            else f_st = 2;
            if (!D_bubble() && !D_stall())
            {
                d_st = 0;
                D_icode = f_icode;
                D_ifun = f_ifun;
                D_rA = f_rA;
                D_rB = f_rB;
                D_valC = f_valC;
                D_valP = f_valP;
            }
            else if (D_bubble() && !D_stall())
            {
                d_st = 1;
                D_icode = 0;
                D_ifun = 0;
                D_rA = Register.none;
                D_rB = Register.none;
                D_valC = 0;
                D_valP = 0;
            }
            else d_st = 2;
            if (!E_bubble())
            {
                e_st = 0;
                E_icode = d_icode;
                E_ifun = d_ifun;
                E_valC = d_valC;
                E_valA = d_valA;
                E_valB = d_valB;
                E_dstE = d_dstE;
                E_dstM = d_dstM;
                E_srcA = d_srcA;
                E_srcB = d_srcB;
            }
            else
            {
                e_st = 1;
                E_icode = 0;
                E_ifun = 0;
                E_valC = 0;
                E_valA = 0;
                E_valB = 0;
                E_dstE = Register.none;
                E_dstM = Register.none;
                E_srcA = Register.none;
                E_srcB = Register.none;
            }
            if (!M_bubble && !M_stall)
            {
                m_st = 0;
                M_icode = e_icode;
                M_Bch = e_Bch;
                M_valE = e_valE;
                M_valA = e_valA;
                M_dstE = e_dstE;
                M_dstM = e_dstM;
            }
            W_icode = m_icode;
            W_valE = m_valE;
            W_valM = m_valM;
            W_dstE = m_dstE;
            W_dstM = m_dstM;
        }
        #endregion

        #region Register_Save
        public void Register_Save(int cycle)
        {
            F_predPCsave[cycle] = F_predPC;
            D_ifunsave[cycle] = D_ifun;
            E_ifunsave[cycle] = E_ifun;
            D_valCsave[cycle] = D_valC;
            D_valPsave[cycle] = D_valP;
            E_valAsave[cycle] = E_valA;
            E_valBsave[cycle] = E_valB;
            E_valCsave[cycle] = E_valC;
            M_valAsave[cycle] = M_valA;
            M_valEsave[cycle] = M_valE;
            W_valEsave[cycle] = W_valE;
            W_valMsave[cycle] = W_valM;
            D_icodesave[cycle] = D_icode;
            E_icodesave[cycle] = E_icode;
            M_icodesave[cycle] = M_icode;
            W_icodesave[cycle] = W_icode;
            D_rAsave[cycle] = D_rA;
            D_rBsave[cycle] = D_rB; 
            E_dstEsave[cycle] = E_dstE;
            M_dstEsave[cycle] = M_dstE;
            W_dstEsave[cycle] = W_dstE;
            E_dstMsave[cycle] = E_dstM;
            M_dstMsave[cycle] = M_dstM;
            W_dstMsave[cycle] = W_dstM;
            E_srcAsave[cycle] = E_srcA;
            E_srcBsave[cycle] = E_srcB;
            eaxsave[cycle] = Dic_regids[Register.eax];
            ecxsave[cycle] = Dic_regids[Register.ecx];
            edxsave[cycle] = Dic_regids[Register.edx];
            ebxsave[cycle] = Dic_regids[Register.ebx];
            espsave[cycle] = Dic_regids[Register.esp];
            ebpsave[cycle] = Dic_regids[Register.ebp];
            esisave[cycle] = Dic_regids[Register.esi];
            edisave[cycle] = Dic_regids[Register.edi];
            ZFsave[cycle] = ZF;
            SFsave[cycle] = SF;
            OFsave[cycle] = OF;
            fetch_pcsave[cycle] = fetch_pc;
            decode_pcsave[cycle] = decode_pc;
            execute_pcsave[cycle] = execute_pc;
            memory_pcsave[cycle] = memory_pc;
            writeback_pcsave[cycle] = writeback_pc;
            f_stsave[cycle] = f_st;
            d_stsave[cycle] = d_st;
            e_stsave[cycle] = e_st;
            m_stsave[cycle] = m_st;
            for (int i = 0; i < maxMemory; i++)
            {
                if (Dic_memory.ContainsKey(i))
                {
                    memorySave[i, cycle] = Dic_memory[i];
                }
                else memorySave[i, cycle] = 0;
            }
            for (int i = 0; i < maxMemory; i++)
            {
                if (Dic_mem.ContainsKey(i))
                {
                    memSave[i, cycle] = Dic_mem[i];
                }
                else memSave[i, cycle] = 0;
            }
        }
        #endregion

        public void print_out(string s)
        {
            FileStream bFile = new FileStream(@s, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(bFile);
            reset();
            int count = 0;
            sw.WriteLine("Cycle_0x{0}", count++);
            sw.WriteLine("--------------------------------");
            sw.WriteLine("FETCH:");
            sw.WriteLine("       F_predPC      = 0x{0:x8}", F_predPC);
            sw.WriteLine("");
            sw.WriteLine("DECODE:");
            sw.WriteLine("       D_icode       = 0x{0:x}", (int)D_icode);
            sw.WriteLine("       D_ifun        = 0x{0:x}", D_ifun);
            sw.WriteLine("       D_rA          = 0x{0:x}", (int)D_rA);
            sw.WriteLine("       D_rB          = 0x{0:x}", (int)D_rB);
            sw.WriteLine("       D_valC        = 0x{0:x8}", D_valC);
            sw.WriteLine("       D_valP        = 0x{0:x8}", D_valP);
            sw.WriteLine("");
            sw.WriteLine("EXECUTE:");
            sw.WriteLine("       E_icode       = 0x{0:x}", (int)E_icode);
            sw.WriteLine("       E_ifun        = 0x{0:x}", E_ifun);
            sw.WriteLine("       E_valC        = 0x{0:x8}", E_valC);
            sw.WriteLine("       E_valA        = 0x{0:x8}", E_valA);
            sw.WriteLine("       E_valB        = 0x{0:x8}", E_valB);
            sw.WriteLine("       E_dstE        = 0x{0:x}", (int)E_dstE);
            sw.WriteLine("       E_dstM        = 0x{0:x}", (int)E_dstM);
            sw.WriteLine("       E_srcA        = 0x{0:x}", (int)E_srcA);
            sw.WriteLine("       E_srcB        = 0x{0:x}", (int)E_srcB);
            sw.WriteLine("");
            sw.WriteLine("MEMORY:");
            sw.WriteLine("       M_icode       = 0x{0:x}", (int)M_icode);
            sw.WriteLine("       M_Bch         = {0}", Convert.ToString(M_Bch).ToLower());
            sw.WriteLine("       M_valE        = 0x{0:x8}", M_valE);
            sw.WriteLine("       M_valA        = 0x{0:x8}", M_valA);
            sw.WriteLine("       M_dstE        = 0x{0:x}", (int)M_dstE);
            sw.WriteLine("       M_dstM        = 0x{0:x}", (int)M_dstM);
            sw.WriteLine("");
            sw.WriteLine("WRITE BACK:");
            sw.WriteLine("       W_icode       = 0x{0:x}", (int)W_icode);
            sw.WriteLine("       W_valE        = 0x{0:x8}", W_valE);
            sw.WriteLine("       W_valM        = 0x{0:x8}", W_valM);
            sw.WriteLine("       W_dstE        = 0x{0:x}", (int)W_dstE);
            sw.WriteLine("       W_dstM        = 0x{0:x}", (int)W_dstM);
            sw.WriteLine("");
            while (W_icode != Instruction.halt)
            {
                Write_Back();
                Memory();
                Execute();
                Decode();
                Fetch();
                Clock();
                sw.WriteLine("Cycle_0x{0}", count++);
                sw.WriteLine("--------------------------------");
                sw.WriteLine("FETCH:");
                sw.WriteLine("       F_predPC      = 0x{0:x8}", F_predPC);
                sw.WriteLine("");
                sw.WriteLine("DECODE:");
                sw.WriteLine("       D_icode       = 0x{0:x}", (int)D_icode);
                sw.WriteLine("       D_ifun        = 0x{0:x}", D_ifun);
                sw.WriteLine("       D_rA          = 0x{0:x}", (int)D_rA);
                sw.WriteLine("       D_rB          = 0x{0:x}", (int)D_rB);
                sw.WriteLine("       D_valC        = 0x{0:x8}", D_valC);
                sw.WriteLine("       D_valP        = 0x{0:x8}", D_valP);
                sw.WriteLine("");
                sw.WriteLine("EXECUTE:");
                sw.WriteLine("       E_icode       = 0x{0:x}", (int)E_icode);
                sw.WriteLine("       E_ifun        = 0x{0:x}", E_ifun);
                sw.WriteLine("       E_valC        = 0x{0:x8}", E_valC);
                sw.WriteLine("       E_valA        = 0x{0:x8}", E_valA);
                sw.WriteLine("       E_valB        = 0x{0:x8}", E_valB);
                sw.WriteLine("       E_dstE        = 0x{0:x}", (int)E_dstE);
                sw.WriteLine("       E_dstM        = 0x{0:x}", (int)E_dstM);
                sw.WriteLine("       E_srcA        = 0x{0:x}", (int)E_srcA);
                sw.WriteLine("       E_srcB        = 0x{0:x}", (int)E_srcB);
                sw.WriteLine("");
                sw.WriteLine("MEMORY:");
                sw.WriteLine("       M_icode       = 0x{0:x}", (int)M_icode);
                sw.WriteLine("       M_Bch         = {0}", Convert.ToString(M_Bch).ToLower());
                sw.WriteLine("       M_valE        = 0x{0:x8}", M_valE);
                sw.WriteLine("       M_valA        = 0x{0:x8}", M_valA);
                sw.WriteLine("       M_dstE        = 0x{0:x}", (int)M_dstE);
                sw.WriteLine("       M_dstM        = 0x{0:x}", (int)M_dstM);
                sw.WriteLine("");
                sw.WriteLine("WRITE BACK:");
                sw.WriteLine("       W_icode       = 0x{0:x}", (int)W_icode);
                sw.WriteLine("       W_valE        = 0x{0:x8}", W_valE);
                sw.WriteLine("       W_valM        = 0x{0:x8}", W_valM);
                sw.WriteLine("       W_dstE        = 0x{0:x}", (int)W_dstE);
                sw.WriteLine("       W_dstM        = 0x{0:x}", (int)W_dstM);
                sw.WriteLine("");
            }
            sw.Close();
        }
    }
}
