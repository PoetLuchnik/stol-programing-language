#include <stdio.h>
#include <stdlib.h>
#include "stolvm.h"
#include "stolvm.c"

int main(int argc, char* argv[])
{
	/* Need a program file */
	if (argc < 2)
		return;
	/* Begin */
	//debug:
	//printf("version: 1.2\n");
	FILE* f = fopen(argv[1], "r");
	fseek(f, 0, SEEK_END);
	ulong_t fc = ftell(f);
	rewind(f);
	byte_t* fv = (byte_t*)malloc(fc * sizeof(byte_t));
	fread(fv, sizeof(byte_t), fc, f);
	fclose(f);
	vm_initialization(fv, fc);
	free(fv);
	/* Work */
	vm_run();
	/* End */
	vm_dispose();
	//getchar();
}