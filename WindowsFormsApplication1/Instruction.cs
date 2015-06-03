using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipeline_Simulator_by_Gu_and_Zhou
{
    public enum Instruction
    {
        nop = 0,
        halt = 1,
        rrmovl = 2,
        irmovl = 3,
        rmmovl = 4,
        mrmovl = 5,
        OPl = 6,
        jXX = 7,
        call = 8,
        ret = 9,
        pushl = 10,
        popl = 11,

    }

    public enum jxx
    {
        jmp = 0,
        jle = 1,
        jl = 2,
        je = 3,
        jne = 4,
        jge = 5,
        jg = 6,
    }

    public enum OPl
    {
        addl = 0,
        subl = 1,
        andl = 2,
        xorl = 3,
    }
    public enum Status { AOK, HLT, ADR, INS }
    public enum Register
    {
        eax = 0,
        ecx = 1,
        edx = 2,
        ebx = 3,
        esp = 4,
        ebp = 5,
        esi = 6,
        edi = 7,
        none = 8
    }
}
