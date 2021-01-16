//Name: STOL Virtual Machine
//Author: Poet Luchnik
//Description: virtual machine for STOL byte code
//--------------------------------------------------------------------------
//Includes:
//#include "easybyte.h"
//--------------------------------------------------------------------------
//Defines:
#define PLB_X 0
#define PLB_Y 1
#define PLB_Z 2
#define PLB_T 3
#define VM_CMD_PROC_END		1
#define VM_CMD_PROC_ID		2
#define VM_CMD_RUN_ID		3
#define VM_CMD_PROC_STOP	4
#define VM_CMD_BYTE_B		5
#define VM_CMD_THIS_B		6
#define VM_CMD_OUT_3C		7
#define VM_CMD_AT_3C		8
#define VM_CMD_CLEAR_OUT	9
#define VM_CMD_PUSH_B		10
#define VM_CMD_POP_B		11
#define VM_CMD_PUSH_C		12
#define VM_CMD_IS_B			13
#define VM_CMD_IS_C			14
#define VM_CMD_NOT_B		15
#define VM_CMD_TO_B			16
#define VM_CMD_POP			17
#define VM_CMD_CLEAR_STACK	18
#define VM_CMD_OUT_B		19
#define VM_CMD_IN_B			20
#define VM_CMD_IN			21
#define VM_CMD_ADD_B		22
#define VM_CMD_SUB_B		23
#define VM_CMD_ADD_C		24
#define VM_CMD_SUB_C		25
#define VM_CMD_INC_B		26
#define VM_CMD_DEC_B		27
#define VM_CMD_PUSH_THISPTR	28
#define VM_CMD_POP_THISPTR	29
#define VM_CMD_MUL_B		30
#define VM_CMD_MUL_C		31
#define VM_CMD_DIV_B		32
#define VM_CMD_DIV_C		33
#define VM_CMD_MOD_B		34
#define VM_CMD_MOD_C		35
#define VM_CMD_PEEK_B		36
#define VM_CMD_IF_C			37
#define VM_CMD_IF_B			38
#define VM_CMD_IF_HAS		39
#define VM_CMD_IF_ELSE		40
#define VM_CMD_IF_END		41
#define VM_CMD_WHILE_C		42
#define VM_CMD_WHILE_B		43
#define VM_CMD_WHILE_HAS	44
#define VM_CMD_WHILE_END	45
/* stream name from stack */
#define VM_CMD_SETOUT		46 //TODO
#define VM_CMD_SETIN		47 //TODO
//
#define VM_CMD_FLIP			48
//
#define VM_CMD_IF_POP		49
#define VM_CMD_WHILE_POP	50
//First cmd: 83 84 79 76 ("STOL")
#define VM_CMD_FILE_TYPE	83
#define VM_MAIN_PROC_ID		0
#define VM_SOMETHING_NOT_FOUND	0
//--------------------------------------------------------------------------
//Typedefs:
typedef unsigned char byte_t;
typedef unsigned long ulong_t;
typedef unsigned int uint_t;
//--------------------------------------------------------------------------
//Structs:
struct Cmd;
struct Link;
struct VirtualMachine;
//--------------------------------------------------------------------------
//Functions:
uint_t uintFrom1b(byte_t x);
uint_t uintFrom2b(byte_t y, byte_t x);
uint_t uintFrom3b(byte_t z, byte_t y, byte_t x);
uint_t uintFrom4b(byte_t t, byte_t z, byte_t y, byte_t x);
byte_t uintAt(uint_t u, byte_t i);
void vm_initialization(byte_t* program, ulong_t plength);
void vm_dispose();
void vm_run();
void vm_putchar(byte_t c);
byte_t vm_getchar();
ulong_t vm_seekWhileEnd(ulong_t whileCmdPtr);
ulong_t vm_seekWhileBegin(ulong_t whileEndCmdPtr);
ulong_t vm_seekIfElseOrEnd(ulong_t ifCmdPtr);
ulong_t vm_seekIfEnd(ulong_t elseCmdPtr);
void vm_while(byte_t boolean);
void vm_if(byte_t boolean);
struct Cmd newCmd(byte_t type, byte_t z, byte_t y, byte_t x);
uint_t cmd_getArgsAsID(struct Cmd* o);
struct Link newLink(uint_t clr, uint_t id);
void ls_clearTopChunk();
uint_t ls_pointerByID(uint_t id);
byte_t ls_isFull();
byte_t ls_isEmpty();
void ls_push(struct Link l);
struct Link ls_pop();
void ls_clear();
byte_t bs_isFull();
byte_t bs_isEmpty();
void bs_push(byte_t b);
byte_t bs_pop();
byte_t bs_pick();
void bs_clear();
byte_t stol_isWhile(byte_t cmdType);
byte_t stol_isIf(byte_t cmdType);
ulong_t stol_runProc(uint_t id, ulong_t lastPointer);
void stol_while_c(byte_t value);
void stol_while_b(uint_t id);
void stol_whileHas();
void stol_whilePop();
void stol_whileEnd();
void stol_if_c(byte_t value);
void stol_if_b(uint_t id);
void stol_ifHas();
void stol_ifPop();
void stol_ifElse();
void stol_ifEnd();
void stol_flip();
void stol_procEnd();
void stol_clearStack();
void stol_inc_b(uint_t id);
void stol_dec_b(uint_t id);
void stol_mod_c(byte_t value);
void stol_mod_b(uint_t id);
void stol_div_c(byte_t value);
void stol_div_b(uint_t id);
void stol_mul_c(byte_t value);
void stol_mul_b(uint_t id);
void stol_add_c(byte_t value);
void stol_add_b(uint_t id);
void stol_sub_c(byte_t value);
void stol_sub_b(uint_t id);
void stol_out_b(uint_t id);
void stol_to_b(uint_t id);
void stol_not_b(uint_t id);
void stol_byte_b(uint_t id);
void stol_this_b(uint_t id);
void stol_is_b(uint_t id);
void stol_is_c(byte_t value);
void stol_push_b(uint_t id);
void stol_push_c(byte_t value);
void stol_pick_b(uint_t id);
void stol_pop_b(uint_t id);
void stol_pop();
void stol_popThisPtr();
void stol_pushThisPtr();
void stol_in_b(uint_t id);
void stol_in();
void stol_out_3c(byte_t z, byte_t y, byte_t x);
void stol_at_3c(byte_t z, byte_t y, byte_t x);
ulong_t seekProcUpper(ulong_t i, uint_t id);
ulong_t seekProcLower(ulong_t i, uint_t id);
ulong_t seekProc(ulong_t i, uint_t id);