using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pipeline_Simulator_by_Gu_and_Zhou
{
    public partial class Form1 : Form
    {
        public const int maxNum = 1000;
        public const int maxMemory = 2048;
        public static Label[,] la = new Label[25, 2]; //label数组，显示反汇编代码
        public Timer time;
        public Pipeline new_pipeline=new Pipeline();
        public int frequence = 1000;  //单位：ms
        public bool is_load = false; //是否载入文件

        public Form1()
        {
            InitializeComponent();
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            tableLayoutPanel1.Controls.Clear();
            for (int i = 0; i < 25; i++)
                for (int j = 0; j < 2; j++)
                {
                    la[i, j] = new Label();
                    la[i, j].Width = 250;
                    tableLayoutPanel1.Controls.Add(la[i, j], j, i);
                }
            textBox.ScrollBars = ScrollBars.Vertical; //垂直滚动条
            r_p.Text = "run";
            new_pipeline.reset();
            setValue();
            f_nop.BackColor = d_nop.BackColor = e_nop.BackColor = m_nop.BackColor = w_nop.BackColor = Color.WhiteSmoke;
        }

        public void colorControl()
        {
            for (int i = 0; i < 25; i++)
            {
                if (string.Format("0x{0:x8}", new_pipeline.writeback_pc) == la[i, 0].Text)
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.Turquoise;
                else if (string.Format("0x{0:x8}", new_pipeline.memory_pc) == la[i, 0].Text)
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.LightGreen;
                else if (string.Format("0x{0:x8}", new_pipeline.execute_pc) == la[i, 0].Text)
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.Khaki;
                else if (string.Format("0x{0:x8}", new_pipeline.decode_pc) == la[i, 0].Text)
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.Plum;
                else if (string.Format("0x{0:x8}", new_pipeline.fetch_pc) == la[i, 0].Text)
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.LightSalmon;
                else
                    la[i, 0].BackColor = la[i, 1].BackColor = Color.WhiteSmoke;
            }
            if (new_pipeline.fetch_pc == -1)
                f_nop.BackColor = Color.LightSalmon;
            else f_nop.BackColor = Color.WhiteSmoke;
            if (new_pipeline.decode_pc == -1)
                d_nop.BackColor = Color.Plum;
            else d_nop.BackColor = Color.WhiteSmoke;
            if (new_pipeline.execute_pc == -1)
                e_nop.BackColor = Color.Khaki;
            else e_nop.BackColor = Color.WhiteSmoke;
            if (new_pipeline.memory_pc == -1)
                m_nop.BackColor = Color.LightGreen;
            else m_nop.BackColor = Color.WhiteSmoke;
            if (new_pipeline.writeback_pc == -1)
                w_nop.BackColor = Color.Turquoise;
            else w_nop.BackColor = Color.WhiteSmoke;
        }

        public void setValue()
        {
            colorControl();
            eax.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.eax]);
            ebx.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.ebx]);
            ecx.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.ecx]);
            edx.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.edx]);
            esi.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.esi]);
            edi.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.edi]);
            esp.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.esp]);
            ebp.Text = string.Format("0x{0:x8}", new_pipeline.Dic_regids[Register.ebp]);
            F_predPC.Text = string.Format("0x{0:x8}", new_pipeline.F_predPC);
            D_icode.Text = Convert.ToString((int)new_pipeline.D_icode);
            D_ifun.Text = Convert.ToString(new_pipeline.D_ifun);
            D_rA.Text = Convert.ToString((int)new_pipeline.D_rA);
            D_rB.Text = Convert.ToString((int)new_pipeline.D_rB);
            D_valC.Text = string.Format("0x{0:x8}", new_pipeline.D_valC);
            D_valP.Text = string.Format("0x{0:x8}", new_pipeline.D_valP);
            E_icode.Text = Convert.ToString((int)new_pipeline.E_icode);
            E_ifun.Text = Convert.ToString(new_pipeline.E_ifun);
            E_valC.Text = string.Format("0x{0:x8}", new_pipeline.E_valC);
            E_valA.Text = string.Format("0x{0:x8}", new_pipeline.E_valA);
            E_valB.Text = string.Format("0x{0:x8}", new_pipeline.E_valB);
            E_dstE.Text = Convert.ToString((int)new_pipeline.E_dstE);
            E_dstM.Text = Convert.ToString((int)new_pipeline.E_dstM);
            E_srcA.Text = Convert.ToString((int)new_pipeline.E_srcA);
            E_srcB.Text = Convert.ToString((int)new_pipeline.E_srcB);
            if (new_pipeline.ZF)
            {
                ZFLabel.Text = "1";
            }
            else ZFLabel.Text = "0";
            if (new_pipeline.SF)
            {
                SFLabel.Text = "1";
            }
            else SFLabel.Text = "0";
            if (new_pipeline.OF)
            {
                OFLabel.Text = "1";
            }
            else OFLabel.Text = "0";
            M_icode.Text = Convert.ToString((int)new_pipeline.M_icode);
            M_Bch.Text = Convert.ToString(new_pipeline.M_Bch);
            M_valE.Text = string.Format("0x{0:x8}", new_pipeline.M_valE);
            M_valA.Text = string.Format("0x{0:x8}", new_pipeline.M_valA);
            M_dstE.Text = Convert.ToString((int)new_pipeline.M_dstE);
            M_dstM.Text = Convert.ToString((int)new_pipeline.M_dstM);
            W_icode.Text = Convert.ToString((int)new_pipeline.W_icode);
            W_valE.Text = string.Format("0x{0:x8}", new_pipeline.W_valE);
            W_valM.Text = string.Format("0x{0:x8}", new_pipeline.W_valM);
            W_dstE.Text = Convert.ToString((int)new_pipeline.W_dstE);
            W_dstM.Text = Convert.ToString((int)new_pipeline.W_dstM);
            cycle.Text = Convert.ToString(new_pipeline.cycle);
            if (new_pipeline.f_st == 2)
                F_flag.Text = "stall";
            else F_flag.Text = "";
            if (new_pipeline.d_st == 1)
                D_flag.Text = "bubble";
            else if (new_pipeline.d_st == 2)
                D_flag.Text = "stall";
            else D_flag.Text = "";
            if (new_pipeline.e_st == 1)
                E_flag.Text = "bubble";
            else if (new_pipeline.e_st == 2)
                E_flag.Text = "stall";
            else E_flag.Text = "";
            int c=new_pipeline.cycle;
            string textb;
            textb = "Address\t\t  Value";
            for (int i = 0; i < maxMemory; i++)
                if (new_pipeline.Dic_memory.ContainsKey(i) && new_pipeline.Dic_memory[i] != 0)
                    textb += Environment.NewLine + string.Format("0x{0:x8}", i) + "\t   " + string.Format("0x{0:x2}", new_pipeline.Dic_memory[i]);
            textBox.Text = textb;
        }

        private void setcycle(int cyc)
        {
            new_pipeline.F_predPC = new_pipeline.F_predPCsave[cyc];
            new_pipeline.D_ifun = new_pipeline.D_ifunsave[cyc];
            new_pipeline.E_ifun = new_pipeline.E_ifunsave[cyc];
            new_pipeline.D_valC = new_pipeline.D_valCsave[cyc];
            new_pipeline.D_valP = new_pipeline.D_valPsave[cyc];
            new_pipeline.E_valA = new_pipeline.E_valAsave[cyc];
            new_pipeline.E_valB = new_pipeline.E_valBsave[cyc];
            new_pipeline.E_valC = new_pipeline.E_valCsave[cyc];
            new_pipeline.M_valA = new_pipeline.M_valAsave[cyc];
            new_pipeline.M_valE = new_pipeline.M_valEsave[cyc];
            new_pipeline.W_valE = new_pipeline.W_valEsave[cyc];
            new_pipeline.W_valM = new_pipeline.W_valMsave[cyc];
            new_pipeline.D_icode = new_pipeline.D_icodesave[cyc];
            new_pipeline.E_icode = new_pipeline.E_icodesave[cyc];
            new_pipeline.M_icode = new_pipeline.M_icodesave[cyc];
            new_pipeline.W_icode = new_pipeline.W_icodesave[cyc];
            new_pipeline.D_rA = new_pipeline.D_rAsave[cyc];
            new_pipeline.D_rB = new_pipeline.D_rBsave[cyc];
            new_pipeline.E_dstE = new_pipeline.E_dstEsave[cyc];
            new_pipeline.M_dstE = new_pipeline.M_dstEsave[cyc];
            new_pipeline.W_dstE = new_pipeline.W_dstEsave[cyc];
            new_pipeline.E_dstM = new_pipeline.E_dstMsave[cyc];
            new_pipeline.M_dstM = new_pipeline.M_dstMsave[cyc];
            new_pipeline.W_dstM = new_pipeline.W_dstMsave[cyc];
            new_pipeline.E_srcA = new_pipeline.E_srcAsave[cyc];
            new_pipeline.E_srcB = new_pipeline.E_srcBsave[cyc];
            new_pipeline.ZF = new_pipeline.ZFsave[cyc];
            new_pipeline.SF = new_pipeline.SFsave[cyc];
            new_pipeline.OF = new_pipeline.OFsave[cyc];
            new_pipeline.Dic_regids[Register.eax] = new_pipeline.eaxsave[cyc];
            new_pipeline.Dic_regids[Register.ecx] = new_pipeline.ecxsave[cyc];
            new_pipeline.Dic_regids[Register.edx] = new_pipeline.edxsave[cyc];
            new_pipeline.Dic_regids[Register.ebx] = new_pipeline.ebxsave[cyc];
            new_pipeline.Dic_regids[Register.esp] = new_pipeline.espsave[cyc];
            new_pipeline.Dic_regids[Register.ebp] = new_pipeline.ebpsave[cyc];
            new_pipeline.Dic_regids[Register.esi] = new_pipeline.esisave[cyc];
            new_pipeline.Dic_regids[Register.edi] = new_pipeline.edisave[cyc];
            new_pipeline.fetch_pc = new_pipeline.fetch_pcsave[cyc];
            new_pipeline.decode_pc = new_pipeline.decode_pcsave[cyc];
            new_pipeline.execute_pc = new_pipeline.execute_pcsave[cyc];
            new_pipeline.memory_pc = new_pipeline.memory_pcsave[cyc];
            new_pipeline.writeback_pc = new_pipeline.writeback_pcsave[cyc];
            new_pipeline.f_st = new_pipeline.f_stsave[cyc];
            new_pipeline.d_st = new_pipeline.d_stsave[cyc];
            new_pipeline.e_st = new_pipeline.e_stsave[cyc];
            new_pipeline.m_st = new_pipeline.m_stsave[cyc];
            for (int i = 0; i < maxMemory; i++)
                if (new_pipeline.Dic_memory.ContainsKey(i))
                    new_pipeline.Dic_memory[i] = (byte) new_pipeline.memorySave[i, cyc];
            for (int i = 0; i < maxMemory; i++)
                if (new_pipeline.Dic_mem.ContainsKey(i))
                    new_pipeline.Dic_mem[i] = new_pipeline.memSave[i, cyc];
           //     else if (new_pipeline.memorysave[i, cyc] != 0)
           //       new_pipeline.Dic_mem.Add(i, new_pipeline.memorysave[i, cyc]);
        }

        #region disassemble

        public string getregid(char c)
        {
            switch (c)
            {
                case '0': return "%eax";
                case '1': return "%ecx";
                case '2': return "%edx";
                case '3': return "%ebx";
                case '4': return "%esp";
                case '5': return "%ebp";
                case '6': return "%esi";
                case '7': return "%edi";
            }
            return "error";
        }

        public string getoper(char c)
        {
            switch (c)
            {
                case '0': return "addl ";
                case '1': return "subl ";
                case '2': return "andl ";
                case '3': return "xorl ";
            }
            return "error";
        }

        public string getjXX(char c)
        {
            switch (c)
            {
                case '0': return "jmp ";
                case '1': return "jle ";
                case '2': return "jl ";
                case '3': return "je ";
                case '4': return "jne ";
                case '5': return "jge ";
                case '6': return "jg ";
            }
            return "error";
        }

        public string getinstr(string s)
        {
            switch (s[0])
            {
                case '0': return "nop";
                case '1': return "halt";
                case '2': return "rrmovl ";
                case '3': return "irmovl ";
                case '4': return "rmmovl ";
                case '5': return "mrmovl ";
                case '6': return getoper(s[1]);
                case '7': return getjXX(s[1]);
                case '8': return "call ";
                case '9': return "ret";
                case 'a': return "pushl ";
                case 'b': return "popl ";
            }
            return "error";
        }

        public string getValC(int i, string s)
        {
            return "0x" + s.Substring(i + 6, 2) + s.Substring(i + 4, 2) + s.Substring(i + 2, 2) + s.Substring(i, 2);
        }

        public string getValCShort(int i, string s)
        {
            string s1 = getValC(i, s);
            int s2 = Convert.ToInt32(s1, 16);
            return "0x" + Convert.ToString(s2);
        }

        public string getCode(string s)
        {
            switch (s[0])
            {
                case '0': return "";
                case '1': return "";
                case '2': return getregid(s[2]) + ", " + getregid(s[3]);
                case '3': return "$" + getValC(4, s) + ", " + getregid(s[3]);
                case '4': return getregid(s[2]) + ", " + getValCShort(4, s) + "(" + getregid(s[3]) + ")";
                case '5': return "" + getValCShort(4, s) + "(" + getregid(s[3]) + "), " + getregid(s[2]);
                case '6': return getregid(s[2]) + ", " + getregid(s[3]);
                case '7': return "0x" + s[8] + s[9] + s[6] + s[7] + s[4] + s[5] + s[2] + s[3];
                case '8': return "0x" + s[8] + s[9] + s[6] + s[7] + s[4] + s[5] + s[2] + s[3];
                case '9': return "";
                case 'a': return getregid(s[2]);
                case 'b': return getregid(s[2]);
            }
            return "error";
        }

        #endregion

        #region Timer_Control

        public void timerTick(object sender, EventArgs e)
        {
            time.Interval = frequence;
            if (new_pipeline.W_icode != Instruction.halt)
            {

                new_pipeline.cycle++;
                new_pipeline.Write_Back();
                new_pipeline.Memory();
                new_pipeline.Execute();
                new_pipeline.Decode();
                new_pipeline.Fetch();
                new_pipeline.Clock();
                new_pipeline.PCupdate();
                new_pipeline.Register_Save(new_pipeline.cycle);
                setValue();

            }
            else
            {
                time.Stop();
                r_p.Text = "run";
                MessageBox.Show("程序已结束");
            }
        }

        public void setFrequence(int f)
        {
            frequence = f;
        }

        private void cpu1_CheckedChanged(object sender, EventArgs e)
        {
            setFrequence(1000);
        }

        private void cpu2_CheckedChanged(object sender, EventArgs e)
        {
            setFrequence(200);
        }

        private void cpu3_CheckedChanged(object sender, EventArgs e)
        {
            setFrequence(100);
        }

        private void cpu4_CheckedChanged(object sender, EventArgs e)
        {
            setFrequence(20);
        }
        #endregion

        #region button_Click

        private void load_Click(object sender, EventArgs e)
        {
            if (time!=null)
            {
                time.Stop();
                r_p.Text = "run";
            }
            OpenFileDialog file = new OpenFileDialog() { Filter = "Y86代码文件(*.yo)|*.yo" };
            file.InitialDirectory = Application.StartupPath;
            file.ShowReadOnly = true;
            DialogResult r = file.ShowDialog();
            if (r == DialogResult.OK)
            {
                if (is_load)
                {
                    for (int i = 0; i < maxMemory; i++)
                        for (int j = 0; j < maxNum; j++)
                            new_pipeline.memSave[i,j] = new_pipeline.memorySave[i,j] = 0;
                    new_pipeline.reset();
                    new_pipeline.Dic_instr.Clear();
                    setValue();
                    colorControl();
                }
                time = new Timer();
                new_pipeline.reset();
                new_pipeline.get_instruction(file.FileName);
                time.Tick += timerTick;
                is_load = true;
                int col = 0;
                foreach (KeyValuePair<int, string> item in new_pipeline.Dic_instr)
                {
                    la[col, 0].Text = string.Format("0x{0:x8}", item.Key);
                    la[col, 1].Text = getinstr(item.Value)+getCode(item.Value);
                    col++;
                }
                colorControl();
                string textb;
                textb = "Address\t\t  Value";
                for (int i = 0; i < maxMemory; i++)
                    if (new_pipeline.Dic_memory.ContainsKey(i) && new_pipeline.Dic_memory[i]!=0)
                        textb += Environment.NewLine + string.Format("0x{0:x8}", i) + "\t   " + string.Format("0x{0:x2}", new_pipeline.Dic_memory[i]);
                textBox.Text = textb;
            }
            file.Dispose();
        }

        private void export_Click(object sender, EventArgs e)
        {
            if (is_load)
            {
                time.Stop();
                r_p.Text = "run";
                int cyc = new_pipeline.cycle;
                new_pipeline.reset();
                SaveFileDialog sfd = new SaveFileDialog() { Filter = "文本文件(*.txt)|*.txt" };
                sfd.InitialDirectory = Application.StartupPath;
                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string localFilePath = sfd.FileName.ToString(); //获得文件路径 
                    new_pipeline.print_out(localFilePath);
                    MessageBox.Show("导出完成！");
                    process1.StartInfo.FileName = localFilePath;
                    process1.Start();
                }
                new_pipeline.reset();                
                time.Stop();
                r_p.Text = "run";
                while (cyc-- >0)
                {
                    if (new_pipeline.W_icode != Instruction.halt)
                    {
                        new_pipeline.cycle++;
                        new_pipeline.Write_Back();
                        new_pipeline.Memory();
                        new_pipeline.Execute();
                        new_pipeline.Decode();
                        new_pipeline.Fetch();
                        new_pipeline.Clock();
                        new_pipeline.Register_Save(new_pipeline.cycle);
                        setValue();
                    }
                    else
                    {
                        time.Stop();
                        r_p.Text = "run";
                        MessageBox.Show("程序已结束");
                    }
                }
                setValue();
            }
            else
            {
                MessageBox.Show("请载入文件");
            }
            
        }

        private void stop_Click(object sender, EventArgs e)
        {
            if (is_load)
            {
                time.Stop();
                new_pipeline.reset();
                new_pipeline.cycle = 0;
                setValue();
                colorControl();
                r_p.Text = "run";
            }
            else
            {
                MessageBox.Show("请载入文件");
            }
        }

        private void forward_Click(object sender, EventArgs e)
        {
            if (is_load)
            {
                time.Stop();
                r_p.Text = "run";
                if (new_pipeline.W_icode != Instruction.halt)
                {
                    new_pipeline.cycle++;
                    new_pipeline.Write_Back();
                    new_pipeline.Memory();
                    new_pipeline.Execute();
                    new_pipeline.Decode();
                    new_pipeline.Fetch();
                    new_pipeline.Clock();
                    new_pipeline.PCupdate();
                    new_pipeline.Register_Save(new_pipeline.cycle);
                    setValue();
                } 
                else
                {
                    time.Stop();
                    r_p.Text = "run";
                    MessageBox.Show("程序已结束");
                }
            }
            else
            {
                MessageBox.Show("请载入文件");
            }
        }

        private void back_Click(object sender, EventArgs e)
        {
            if (is_load)
            {
                time.Stop();
                r_p.Text = "run";
                if (new_pipeline.cycle > 0) 
                    new_pipeline.cycle--;
                else MessageBox.Show("已经回退至程序开头，无法继续回退");
                setcycle(new_pipeline.cycle);
                setValue();
            }
            else
            {
                MessageBox.Show("请载入文件");
            }
        }

        private void r_p_Click(object sender, EventArgs e)
        {
            if (r_p.Text == "run")
            {
                if (is_load)
                {
                    time.Interval = frequence;
                    time.Start();
                    r_p.Text = "pause";
                }
                else
                {
                    MessageBox.Show("请载入文件");
                }
            }
            else
            {
                time.Stop();
                r_p.Text = "run";
            }

        }

        #endregion
    }
}
