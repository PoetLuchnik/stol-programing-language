//Name: STOL Virtual Machine
//Author: Poet Luchnik
//Description: virtual machine for STOL byte code
//--------------------------------------------------------------------------
//Includes:
//#include <stdio.h>
//--------------------------------------------------------------------------
//Structs:
struct Cmd
{
	/* Command type */
	byte_t type;
	/* 1st argument */
	byte_t z;
	/* 2nd argument */
	byte_t y;
	/* 3st argument */
	byte_t x;
};
struct Link
{
	/* Color mark to see difference between to links in difference processes */
	uint_t clr : 8;
	/* Unique link number identifier */
	uint_t id : 24;
};
struct VirtualMachine 
{ 
	/* Random-access memory */
	byte_t* ram;
	/* RAM byte count */
	uint_t ram_size;
	/* Temp RAM link */
	uint_t ram_this;
	/* Program */
	struct Cmd* prg;
	/* Program command count */
	ulong_t prg_length;
	/* Command pointer */
	ulong_t cp;
	/* Link stack */
	struct Link* ls;
	/* Link stack max count of links */
	uint_t ls_capacity;
	/* Index of top element in link stack */
	int ls_top;
	/* Color mark for next link in stack */
	uint_t ls_nextColor;
	/* Byte stack */
	byte_t* bs;
	/* Byte stack max count of bytes */
	uint_t bs_capacity;
	/* Index of top element in byte stack */
	int bs_top;
} vm;
//--------------------------------------------------------------------------
//Functions:
/* Execute any process */
ulong_t stol_runProc(uint_t id, ulong_t lastPointer)
{
	//Try:
	//printf("\n>proc: %i\t> ", id);
	vm.cp = seekProc(lastPointer, id);
	if (vm.cp == VM_SOMETHING_NOT_FOUND)
		return lastPointer;
	//Process:
	vm.cp++;
	vm.ls_nextColor = !vm.ls_nextColor;
	while (vm.cp < vm.prg_length)
	{
		struct Cmd* c = vm.prg + vm.cp;
		//DEBUG printf:
		//printf("\n>%i\t(%i, %i, %i)\t> ", c->type, c->z, c->y, c->x);
		switch (c->type)
		{
		case VM_CMD_CLEAR_STACK: stol_clearStack(); break;
		case VM_CMD_AT_3C: stol_at_3c(c->z, c->y, c->x); break;
		case VM_CMD_OUT_3C: stol_out_3c(c->z, c->y, c->x); break;
		case VM_CMD_THIS_B: stol_this_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_OUT_B: stol_out_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_BYTE_B: stol_byte_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_PUSH_B: stol_push_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_PUSH_C: stol_push_c(c->x); break;
		case VM_CMD_ADD_B: stol_add_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_ADD_C: stol_add_c(c->x); break;
		case VM_CMD_SUB_B: stol_sub_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_SUB_C: stol_sub_c(c->x); break;
		case VM_CMD_IS_B: stol_is_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_IS_C: stol_is_c(c->x); break;
		case VM_CMD_PEEK_B: stol_pick_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_POP_B: stol_pop_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_POP: stol_pop(); break;
		case VM_CMD_NOT_B: stol_not_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_TO_B: stol_to_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_INC_B: stol_inc_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_DEC_B: stol_dec_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_IN_B: stol_in_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_IN: stol_in(); break;
		case VM_CMD_POP_THISPTR: stol_popThisPtr(); break;
		case VM_CMD_PUSH_THISPTR: stol_pushThisPtr(); break;
		case VM_CMD_MUL_B: stol_mul_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_MUL_C: stol_mul_c(c->x); break;
		case VM_CMD_DIV_B: stol_div_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_DIV_C: stol_div_c(c->x); break;
		case VM_CMD_MOD_B: stol_mod_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_MOD_C: stol_mod_c(c->x); break;
		case VM_CMD_IF_B: stol_if_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_IF_C: stol_if_c(c->x); break;
		case VM_CMD_IF_HAS: stol_ifHas(); break;
		case VM_CMD_IF_POP: stol_ifPop(); break; //
		case VM_CMD_IF_END: stol_ifEnd(); break;
		case VM_CMD_IF_ELSE: stol_ifElse(); break;
		case VM_CMD_WHILE_B: stol_while_b(cmd_getArgsAsID(c)); break;
		case VM_CMD_WHILE_C: stol_while_c(c->x); break;
		case VM_CMD_WHILE_HAS: stol_whileHas(); break;
		case VM_CMD_WHILE_END: stol_whileEnd(); break;
		case VM_CMD_FLIP: stol_flip(); break;
		case VM_CMD_WHILE_POP: stol_whilePop(); break; //
		case VM_CMD_RUN_ID: vm.cp = stol_runProc(cmd_getArgsAsID(c), vm.cp) + 1; break;
		case VM_CMD_PROC_END: 
		case VM_CMD_PROC_STOP: 
		default: stol_procEnd(); break;
		}
	}
	//The End:
	ls_clearTopChunk();
	vm.ls_nextColor = !vm.ls_nextColor;
	return lastPointer;
}
/* Get the command pointer in begin of while block */
ulong_t vm_seekWhileBegin(ulong_t whileEndCmdPtr)
{
	int lvl = 0;
	whileEndCmdPtr--;
	while (whileEndCmdPtr > 0)
	{
		byte_t t = vm.prg[whileEndCmdPtr].type;
		if (lvl == 0 && stol_isWhile(t))
			return whileEndCmdPtr;

		if (t == VM_CMD_WHILE_END)
			lvl++;
		if (stol_isWhile(t))
			lvl--;

		whileEndCmdPtr--;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Get the command pointer in end of while block */
ulong_t vm_seekWhileEnd(ulong_t whileCmdPtr)
{
	int lvl = 0;
	whileCmdPtr++;
	while (whileCmdPtr < vm.prg_length)
	{
		byte_t t = vm.prg[whileCmdPtr].type;
		if (lvl == 0 && t == VM_CMD_WHILE_END)
			return whileCmdPtr;
		if (stol_isWhile(t))
			lvl++;
		if (t == VM_CMD_WHILE_END)
			lvl--;

		whileCmdPtr++;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Run any while block */
void vm_while(byte_t boolean)
{
	if (boolean)
	{
		vm.cp++;
	}
	else
	{
		vm.cp = vm_seekWhileEnd(vm.cp) + 1;
	}
}
/* Call while by CONST param */
void stol_while_c(byte_t value)
{
	vm_while(value != 0);
}
/* Call while by BYTE param */
void stol_while_b(uint_t id)
{
	vm_while(vm.ram[ls_pointerByID(id)] != 0);
}
/* Call while by HAS param */
void stol_whileHas()
{
	vm_while(!bs_isEmpty());
}
/* Call while by POP param */
void stol_whilePop()
{
	vm_while(bs_pop() != 0);
}
/* The end of while */
void stol_whileEnd()
{
	vm.cp = vm_seekWhileBegin(vm.cp);
}
/* Get true if this cmd type is 'while' command variant */
byte_t stol_isWhile(byte_t cmdType)
{
	return	cmdType == VM_CMD_WHILE_B ||
			cmdType == VM_CMD_WHILE_C ||
			cmdType == VM_CMD_WHILE_POP ||
			cmdType == VM_CMD_WHILE_HAS;
}
/* Get true if this cmd type is 'if' command variant */
byte_t stol_isIf(byte_t cmdType)
{
	return	cmdType == VM_CMD_IF_B || 
			cmdType == VM_CMD_IF_C || 
			cmdType == VM_CMD_IF_POP || 
			cmdType == VM_CMD_IF_HAS;
}
/* Get the command pointer in end of if block */
ulong_t vm_seekIfEnd(ulong_t elseCmdPtr)
{
	int lvl = 0;
	elseCmdPtr++;
	while (elseCmdPtr < vm.prg_length)
	{
		byte_t t = vm.prg[elseCmdPtr].type;
		if (lvl == 0 && t == VM_CMD_IF_END)
			return elseCmdPtr;
		if (stol_isIf(t))
			lvl++;
		if (t == VM_CMD_IF_END)
			lvl--;

		elseCmdPtr++;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Get the else command pointer for this if block */
ulong_t vm_seekIfElseOrEnd(ulong_t ifCmdPtr)
{
	int lvl = 0;
	ifCmdPtr++;
	while (ifCmdPtr < vm.prg_length)
	{
		byte_t t = vm.prg[ifCmdPtr].type;
		if (lvl == 0 && (t == VM_CMD_IF_ELSE || t == VM_CMD_IF_END))
				return ifCmdPtr;
		if (stol_isIf(t))
			lvl++;
		if (t == VM_CMD_IF_END)
			lvl--;

		ifCmdPtr++;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Run any if block */
void vm_if(byte_t boolean)
{
	if (boolean)
	{
		vm.cp++;
	}
	else
	{
		vm.cp = vm_seekIfElseOrEnd(vm.cp) + 1;
	}
}
/* Call if by CONST param */
void stol_if_c(byte_t value)
{
	vm_if(value != 0);
}
/* Call if by BYTE param */
void stol_if_b(uint_t id)
{
	vm_if(vm.ram[ls_pointerByID(id)] != 0);
}
/* Call if by POP param */
void stol_ifPop()
{
	vm_if(bs_pop() != 0);
}
/* Call if by HAS param */
void stol_ifHas()
{
	vm_if(!bs_isEmpty());
}
/* Begin of else block */
void stol_ifElse()
{
	vm.cp = vm_seekIfEnd(vm.cp) + 1;
}
/* End of if block */
void stol_ifEnd()
{
	vm.cp++;
}
/* Flip the byte stack */
void stol_flip()
{
	int p1 = 0, p2 = vm.bs_top;
	while (p1 < p2)
	{
		byte_t tmp = vm.bs[p1];
		vm.bs[p1] = vm.bs[p2];
		vm.bs[p2] = tmp;
		p1++;
		p2--;
	}
	vm.cp++;
}
/* Set byte to top elemet from stack */
void stol_pick_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)] = bs_pick();
	vm.cp++;
}
/* This %= value */
void stol_mod_c(byte_t value)
{
	vm.ram[vm.ram_this] %= value;
	vm.cp++;
}
/* This %= byte */
void stol_mod_b(uint_t id)
{
	vm.ram[vm.ram_this] %= vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* This /= value */
void stol_div_c(byte_t value)
{
	vm.ram[vm.ram_this] /= value;
	vm.cp++;
}
/* This /= byte */
void stol_div_b(uint_t id)
{
	vm.ram[vm.ram_this] /= vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* This *= value */
void stol_mul_c(byte_t value)
{
	vm.ram[vm.ram_this] *= value;
	vm.cp++;
}
/* This *= byte */
void stol_mul_b(uint_t id)
{
	vm.ram[vm.ram_this] *= vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* Set this pointer from 3 bytes in top of stack */
void stol_popThisPtr()
{
	byte_t z = bs_pop();
	byte_t y = bs_pop();
	byte_t x = bs_pop();
	vm.ram_this = uintFrom3b(z, y, x);
	vm.cp++;
}
/* Push this pointer as 3 bytes to stack */
void stol_pushThisPtr()
{
	bs_push(uintAt(vm.ram_this, 2));
	bs_push(uintAt(vm.ram_this, 1));
	bs_push(uintAt(vm.ram_this, 0));
	vm.cp++;
}
/* Given byte++ */
void stol_inc_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)]++;
	vm.cp++;
}
/* Given byte-- */
void stol_dec_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)]--;
	vm.cp++;
}
/* This byte += given byte */
void stol_add_c(byte_t value)
{
	vm.ram[vm.ram_this] += value;
	vm.cp++;
}
/* This byte -= given byte */
void stol_sub_c(byte_t value)
{
	vm.ram[vm.ram_this] -= value;
	vm.cp++;
}
/* This byte += given byte */
void stol_add_b(uint_t id)
{
	vm.ram[vm.ram_this] += vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* This byte -= given byte */
void stol_sub_b(uint_t id)
{
	vm.ram[vm.ram_this] -= vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* Set byte to code of pressed key */
void stol_in_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)] = vm_getchar();
	vm.cp++;
}
/* Wait key */
void stol_in()
{
	vm_getchar();
	vm.cp++;
}
/* Output byte as char */
void stol_out_b(uint_t id)
{
	vm_putchar(vm.ram[ls_pointerByID(id)]);
	vm.cp++;
}
/* Remove all stack elements */
void stol_clearStack()
{
	bs_clear();
	vm.cp++;
}
/* Set given byte to this byte */
void stol_to_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)] = vm.ram[vm.ram_this];
	vm.cp++;
}
/* Invert all bits in byte */
void stol_not_b(uint_t id)
{
	uint_t p = ls_pointerByID(id);
	vm.ram[p] = !vm.ram[p];
	vm.cp++;
}
/* Set this byte */
void stol_is_b(uint_t id)
{
	vm.ram[vm.ram_this] = vm.ram[ls_pointerByID(id)];
	vm.cp++;
}
/* Set this byte */
void stol_is_c(byte_t value)
{
	vm.ram[vm.ram_this] = value;
	vm.cp++;
}
/* Push const value to stack */
void stol_push_c(byte_t value)
{
	bs_push(value);
	vm.cp++;
}
/* Push byte value to stack */
void stol_push_b(uint_t id)
{
	bs_push(vm.ram[ls_pointerByID(id)]);
	vm.cp++;
}
/* Remove stack top element */
void stol_pop()
{
	bs_pop();
	vm.cp++;
}
/* Pop value from stack and save to byte */
void stol_pop_b(uint_t id)
{
	vm.ram[ls_pointerByID(id)] = bs_pop();
	vm.cp++;
}
/* Stop this process */
void stol_procEnd()
{
	vm.cp = vm.prg_length;
}
/* Create new link on byte */
void stol_byte_b(uint_t id)
{
	ls_push(newLink(vm.ls_nextColor, id));
	vm.ram_this = ls_pointerByID(id);
	vm.cp++;
}
/* Set main pointer */
void stol_this_b(uint_t id)
{
	vm.ram_this = ls_pointerByID(id);
	vm.cp++;
}
/* Print to 3 bytes */
void stol_out_3c(byte_t z, byte_t y, byte_t x)
{
	vm_putchar(z);
	vm_putchar(y);
	vm_putchar(x);
	vm.cp++;
}
/* Set main pointer from XYZ */
void stol_at_3c(byte_t z, byte_t y, byte_t x)
{
	vm.ram_this = uintFrom3b(z, y, x);
	vm.cp++;
}
//--- Forget zone... ----------------------------------------------------
/* Malloc, setup values & setup program */
void vm_initialization(byte_t* prgv, ulong_t prgl)
{
	//RAM:
	vm.ram_size = 16777216;
	vm.ram_this = 0;
	vm.ram = (byte_t*)malloc(vm.ram_size * sizeof(byte_t));
	//Program:
	vm.prg_length = prgl / 4;
	vm.prg = (struct Cmd*)malloc(prgl * sizeof(byte_t));
	for (ulong_t i = 0; i < vm.prg_length; i++)
	{
		ulong_t p = i * 4;
		vm.prg[i] = newCmd(prgv[p], prgv[p + 1], prgv[p + 2], prgv[p + 3]);
	}
	vm.cp = 0;
	//Link stack:
	vm.ls_nextColor = 0;
	vm.ls_capacity = 65536;
	vm.ls_top = -1;
	vm.ls = (struct Link*)malloc(vm.ls_capacity * sizeof(struct Link));
	//Byte stack:
	vm.bs_capacity = 65536;
	vm.bs_top = -1;
	vm.bs = (byte_t*)malloc(vm.bs_capacity * sizeof(byte_t));
}
/* Get char from input stream */
byte_t vm_getchar()
{
	return getch();
}
/* Run main proc */
void vm_run()
{
	stol_runProc(VM_MAIN_PROC_ID, vm.prg_length);
}
/* Free all pointers */
void vm_dispose()
{
	free(vm.ram);
	free(vm.prg);
	free(vm.ls);
	free(vm.bs);
}
/* Get proc pointer upper or NOT_FOUND if proc wasn't found */
ulong_t seekProcUpper(ulong_t i, uint_t id)
{
	while (i > 0)
	{
		struct Cmd* c = vm.prg + i;
		if (c->type == VM_CMD_PROC_ID && cmd_getArgsAsID(c) == id)
			return i;
		i--;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Get proc pointer lower or NOT_FOUND if proc wasn't found */
ulong_t seekProcLower(ulong_t i, uint_t id)
{
	while (i < vm.prg_length)
	{
		struct Cmd* c = vm.prg + i;
		if (c->type == VM_CMD_PROC_ID && cmd_getArgsAsID(c) == id)
			return i;
		i++;
	}
	return VM_SOMETHING_NOT_FOUND;
}
/* Get proc pointer or NOT_FOUND if proc wasn't found */
ulong_t seekProc(ulong_t i, uint_t id)
{
	ulong_t r = seekProcUpper(i, id);
	return r == VM_SOMETHING_NOT_FOUND ? seekProcLower(i, id) : r;
}
/* Get true is byte stack is full */
byte_t bs_isFull()
{
	return vm.bs_top == vm.bs_capacity - 1;
}
/* Get true is byte stack is empty */
byte_t bs_isEmpty()
{
	return vm.bs_top == -1;
}
/* Push byte to stack */
void bs_push(byte_t b)
{
	if (bs_isFull())
		return;

	vm.bs_top++;
	vm.bs[vm.bs_top] = b;
}
/* Pop byte from stack */
byte_t bs_pop()
{
	if (bs_isEmpty())
		return 0;

	vm.bs_top--;
	return vm.bs[vm.bs_top + 1];
}
/* Get top byte from stack */
byte_t bs_pick()
{
	if (bs_isEmpty())
		return 0;

	return vm.bs[vm.bs_top];
}
/* Clear byte stack */
void bs_clear()
{
	vm.bs_top = -1;
}
/* Print byte as char */
void vm_putchar(byte_t c)
{
	if (c != 0)
		putc(c, stdout);
}
/* Clear all top links by color */
void ls_clearTopChunk()
{
	if (ls_isEmpty())
		return;

	uint_t clr = vm.ls[vm.ls_top].clr;
	while (vm.ls[vm.ls_top].clr == clr && vm.ls_top > -1)
		vm.ls_top--;
}
/* Get byte pointer by ID */
uint_t ls_pointerByID(uint_t id)
{
	int i = vm.ls_top;

	while (i > -1)
	{
		if (vm.ls[i].id == id)
			return (uint_t)i;
		i--;
	}

	return 0;
}
/* Get true is link stack is full */
byte_t ls_isFull()
{
	return vm.ls_top == vm.ls_capacity - 1;
}
/* Get true is link stack is empty */
byte_t ls_isEmpty()
{
	return vm.ls_top == -1;
}
/* Push link to stack */
void ls_push(struct Link l)
{
	if (ls_isFull())
		return;

	vm.ls_top++;
	vm.ls[vm.ls_top] = l;
}
/* Pop link from stack */
struct Link ls_pop()
{
	if (ls_isEmpty())
		return newLink(0, 0);

	vm.ls_top--;
	return vm.ls[vm.ls_top + 1];
}
/* Clear link stack */
void ls_clear()
{
	vm.ls_top = -1;
}
/* Create new instruction by 4 bytes */
struct Cmd newCmd(byte_t type, byte_t z, byte_t y, byte_t x)
{
	struct Cmd n;
	n.type = type;
	n.z = z;
	n.y = y;
	n.x = x;
	return n;
}
/* Get instruction args as id */
uint_t cmd_getArgsAsID(struct Cmd* o)
{
	return uintFrom3b(o->z, o->y, o->x);
}
/* Create new link by 3 uint */
struct Link newLink(uint_t clr, uint_t id)
{
	struct Link l;
	l.clr = clr;
	l.id = id;
	return l;
}
/* Create uint from 1 bytes */
uint_t uintFrom1b(byte_t x)
{
	return uintFrom4b(0, 0, 0, x);
}
/* Create uint from 2 bytes */
uint_t uintFrom2b(byte_t y, byte_t x)
{
	return uintFrom4b(0, 0, y, x);
}
/* Create uint from 3 bytes */
uint_t uintFrom3b(byte_t z, byte_t y, byte_t x)
{
	return uintFrom4b(0, z, y, x);
}
/* Create uint from 4 bytes */
uint_t uintFrom4b(byte_t t, byte_t z, byte_t y, byte_t x)
{
	return ((t * 256 + z) * 256 + y) * 256 + x;
}
/* Int as array of bytes */
byte_t uintAt(uint_t u, byte_t i)
{
	return (u >> (8 * i)) & 0xFF;
}