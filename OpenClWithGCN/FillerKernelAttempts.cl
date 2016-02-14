
// This file contains Filler Kernel Attempts.

// The code below is not used in the program but it does show some different versions of a dummy kernels.



#define SREG_CT 10
#define VREG_CT 10
#define PARAM_CT 2 // range:2-38  ;ct=8+n
#define SIZE_CT 20  // range:0-10000

// This is a later Dummy kernel for use with __asm4GCN kernels (not inline)
 __kernel void DummyFillerKernel(
#if(PARAM_CT>0)
__global float *i0 
#if(PARAM_CT>1)
,__global float *i1 
#if(PARAM_CT>2)
,__global float *i2 
#if(PARAM_CT>3)
,__global float *i3 
#if(PARAM_CT>4)
,__global float *i4 
#if(PARAM_CT>5)
,__global float *i5 
#if(PARAM_CT>6)
,__global float *i6 
#if(PARAM_CT>7)
,__global float *i7 
#endif
#endif 
#endif
#endif
#endif 
#endif 
#endif
#endif
) 
 { 
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);	barrier(0);	barrier(1);
	size_t gid = get_global_id(0)+0x789ABCDC;
	size_t lid = get_local_id(1)+0x789ABCDD;
	int kernelID = gid+78;
	float x= *(int*)&(kernelID);  							 // An ID for this kernel; 0- 1000
#if(PARAM_CT>0)
	float f0 = x +=*i0;

#if(PARAM_CT>1)
	float f1 = x +=*i1; 
#if(PARAM_CT>2)
	float f2 = x +=*i2; 
#if(PARAM_CT>3)
	float f3 = x +=*i3; 
#if(PARAM_CT>4)
	float f4 = x +=*i4; 
#if(PARAM_CT>5)
	float f5 = x +=*i5; 
#if(PARAM_CT>6)
	float f6 = x +=*i6; 
#if(PARAM_CT>7)
	float f7 = x +=*i7; 
#endif
#endif 
#endif
#endif
#endif 
#endif 
#endif
#endif

	// Find inputs
	//x += (uint)f0|(uint)f1|(uint)f2|(uint)f3|(uint)f4|(uint)f5|(uint)f6|(uint)f7;//(uint)(*f0).s0 | (uint)(*f1).s0 | (uint)(*f2).s0 | (uint)(*f3).s0 | (uint)(*f4).s0  | (uint)(*f5).s0 | (uint)(*f6).s0 | (uint)(*f7).s0 ;
	 
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);	barrier(0);	barrier(1);
	
	
	//const uint SREG_CT = 10; // 
	//const uint VREG_CT = 10; // range:2-38  ;ct=8+n
	//const uint SIZE_CT = 20; // range:0-10000
	

	__local float temp[100];
	temp[lid] = (float)(lid<<gid);
  x += temp[gid];
		
	//#pragma unroll 1
	#if(SREG_CT>0)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>1)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>2)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>3)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>4)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>5)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>6)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>7)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>8)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>9)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>10)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>11)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>12)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>13)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>14)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>15)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>16)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>17)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>18)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>19)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>20)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>21)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>22)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>23)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>24)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>25)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>26)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>27)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>28)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#if(SREG_CT>29)
	for(int a=0x789ABCDE; a>0; a-=0x35A)
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	#endif
	{
		

		// Process VREG_CT
		if (VREG_CT>0)
		{
			uint t[VREG_CT] = {0};
			t[get_local_id(0)]=t[get_local_id(0)+1];
		}

		// Add to kernel's overall size
		float tmp = *(float*)&x;
		#pragma unroll 
		for(long i=0; i<SIZE_CT; i++)
			tmp = rsqrt(tmp);
		x = *(uint*)&tmp;
	}

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);


	i0[0x0000FFFF] = x ;
	
};

///////////////////////////////////////////////////////////////////////////////////////////


// This is dummy filler code for inline asm.  To use it just call fill in the SREG_CT, 
// VREG_CT, and SIZE_CT with defines and then call this function where it is needed. OpenCL 
// will automatically inline the code for us.
 void DummyFillerCode(int i0, double i1, char i2, int *i3, int i4)
 {
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	uint x=77; 							 // An ID for this kernel; 0- 1000
	
	uint vReg4_0 = 10;
	uint vReg4_1 = 10;
	uint vReg4_2 = 10;
	uint vReg8_3 = 10;
	uint vReg8_4 = 10;

	#pragma unroll 1
	for(int i=0x789ABCDE; i>0; i-=0x35A)
	{
		// make sure to use the inputs.  This will also help us match-up the registers used for the inputs.
		x += (*(uint*)i0+x) | (*(uint*)i1+x) | (*(uint*)i2+x) | (*(uint*)i3+x) | (*(uint*)i4+x);

		// Process vReg4[3]
		if (VREG_CT>0)
		{
			uint t[3] = {0};
			t[0]= x  << get_local_id(0);
			for(int i=0; i<VREG_CT; i++)
				t[i+1]= t[i] << get_local_id(0);
			t[VREG_CT] = min(t[VREG_CT-1], (uint)1);
			for(int i=0; i<VREG_CT; i++)
				t[i+(VREG_CT+1)] = min(t[i+VREG_CT], t[(VREG_CT-2)-i]);
			x  = max(t[VREG_CT*2-1], x);
		}

		// Process SREG_CT
		if (SREG_CT>0)
		{
			uint s[SREG_CT*2+1] = {0};
			for(s[0]=0; s[0] < (SREG_CT>0?x:0)   ; s[0]++)
			for(s[1]=0; s[1] < (SREG_CT>0?s[0]:0); s[1]++)
			for(s[2]=0; s[2] < (SREG_CT>1?s[1]:0); s[2]++)
			for(s[3]=0; s[3] < (SREG_CT>2?s[2]:0); s[3]++)
			for(s[4]=0; s[4] < (SREG_CT>3?s[3]:0); s[4]++)
			for(s[5]=0; s[5] < (SREG_CT>4?s[4]:0); s[5]++)
				x++;
		}

		// Add to kernel's overall size
		float tmp = *(float*)&x;
		#pragma unroll 
		for(long i=0; i<SIZE_CT; i++)
			tmp = rsqrt(tmp);
		x = *(uint*)&tmp;
	}

	(*(uint*)i0) = (*(uint*)i1) = (*(uint*)i2) = (*(uint*)i3) = (*(uint*)i4) = x;

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
 }
 
   
 __kernel void ExampleCode2(__global my_struct* input, __global float* output) 
 { 
	size_t i = get_local_id(0);
	int i0 = 5; double i1=1.1f; char i2 = 3; int *i3 = 0; int i4 =5;
	DummyFillerCode(i0,i1,i2,i3,&i4); // <---- This would be the line
	output[i] =f0 + f1;
};



 // the goal is to create a function for use as a template. Use: dummyKernel77(&f0,&f1,&f2,&f3,&f4,&f5,&f6,&f7);
void DummyFillerCode1( void *f0, void *f1, void *f2, void *f3, void *f4, void *f5, void *f6, void *f7 ) 
{ 
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	
	const uint SREG_CT = 10; // 
	const uint VREG_CT = 10; // range:2-38  ;ct=8+n
	const uint SIZE_CT = 20; // range:0-10000
	uint x=77; 							 // An ID for this kernel; 0- 1000
	
	#pragma unroll 1
	for(int i=0x789ABCDE; i>0; i-=0x35A)
	{
		// Find inputs
		x += (*(uint*)f0+x) | (*(uint*)f1+x) | (*(uint*)f2+x) | (*(uint*)f3+x) | (*(uint*)f4+x)  | (*(uint*)f5+x) | (*(uint*)f6+x) | (*(uint*)f7+x) ;
		
		// Process VREG_CT
		if (VREG_CT>0)
		{
			uint  t[VREG_CT*2+1] = {0};
			t[0]= x  << get_local_id(0);
			for(int i=0; i<VREG_CT; i++)
				t[i+1]= t[i] << get_local_id(0);
			t[VREG_CT] = min(t[VREG_CT-1], (uint)1);
			for(int i=0; i<VREG_CT; i++)
				t[i+(VREG_CT+1)] = min(t[i+VREG_CT], t[(VREG_CT-2)-i]);
			x  = max(t[VREG_CT*2-1], x);
		}

		// Process SREG_CT
		if (SREG_CT>0)
		{
			uint s[SREG_CT*2+1] = {0};
			for(s[0]=0; s[0] < (SREG_CT>0?x:0)   ; s[0]++)
			for(s[1]=0; s[1] < (SREG_CT>0?s[0]:0); s[1]++)
			for(s[2]=0; s[2] < (SREG_CT>1?s[1]:0); s[2]++)
			for(s[3]=0; s[3] < (SREG_CT>2?s[2]:0); s[3]++)
			for(s[4]=0; s[4] < (SREG_CT>3?s[3]:0); s[4]++)
			for(s[5]=0; s[5] < (SREG_CT>4?s[4]:0); s[5]++)
				x++;
		}

		// Add to kernel's overall size
		float tmp = *(float*)&x;
		#pragma unroll 
		for(long i=0; i<SIZE_CT; i++)
			tmp = rsqrt(tmp);
		x = *(uint*)&tmp;
	}

	(*(uint*)f0) = (*(uint*)f1) = (*(uint*)f2) = (*(uint*)f3) = (*(uint*)f4)  = (*(uint*)f5) = (*(uint*)f6) = (*(uint*)f7) =x;

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

};
 
 __kernel void square(__global my_struct* input, __global float* output) 
 { 
	size_t i = get_local_id(0);
	float f0 = input[i+0].a;
	float f1 = input[i+1].a; 
	float f2 = input[i+2].a; 
	float f3 = input[i+3].a; 
	float f4 = input[i+4].a; 
	float f5 = input[i+5].a; 
	float f6 = input[i+6].a; 
	float f7 = input[i+7].a; 
	//f1 += f2;
	//f2 *= f3;
	//f0 += f1;
	//f2 += f0;
	 
	DummyFillerCode1(&f0,&f1,&f2,&f3,&f4,&f5,&f6,&f7);

	output[i] =f0 + f1;
};



////////////////////////////////////////////////////////////////////
#define VREG_CT 200
#define SIZE_CT 27
__attribute__((reqd_work_group_size(64,1,1)))
__kernel void myAsmFunc(uint i0,uint i1)
 { 
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

	 union {uint i; uint u; float f; uint *up; float *fp;} x;
	//float *f =  (float *)0x28ff44;
	x.u = 0x789ABCDF;
	//x.f= x.fp[0x789ABCDE];  							 // An ID for this kernel; 0- 1000

	// Find inputs
  x.u <<= i0;x.u <<= i1;

	// Process VREG_CT
	uint temp[VREG_CT];
	for (int i = 0; i < VREG_CT; i++)
		temp[i] = 0x789AC000 + i;
	x.u ^= temp[x.u];	 

	x.u &= 0x789AB000;  							 // An ID for this kernel; 0- 1000
	x.u ^= get_local_id(1);
	x.u &= get_global_id(0); 
	x.u |= get_group_id(0); 

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);



	{
		 if (x.u < 0xFFFFFFF0) return; // prevents hang in case we accidently run this
		
		// Add to kernel's overall size
		if(SIZE_CT>100)
    {
			float tmp = *(float*)&x;
			#pragma unroll 
			for(long i=0; i<SIZE_CT-100; i++)
				tmp = rsqrt(tmp);
			x.f = *(uint*)&tmp;
     }
	}

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

	x.fp[0x0000FFFF] = x.f;
};



///////////////////////////////////////////////////////////////
#define SREG_CT 4
#define VREG1_CT 5
#define VREG2_CT 5
#define VREG4_CT 5
#define VREG8_CT 5
#define SIZE_CT 130

void myAsmKernel000(float volatile i0, uint volatile i1, uint volatile i2 )
{
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(255);
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	int id = get_local_id(0) % 64;
	//float *f =  (float *)0x28ff44;
	volatile union {int i; uint u; float f; uint *up; float *fp;} x;
	x.f = id; 
	x.u ^= 15;  //ID for this kernel: -15 to 64
	x.u ^= 20;  //some ID #2 (-15 to 64)
	x.u ^= 30;  //some ID #3 (-15 to 64)

	// Find inputs
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
  x.u ^= *(uint*)&i0 + 1; 
	x.u ^= *(uint*)&i1 + 2;
	x.u ^= *(uint*)&i2 + 3;
    
	// Process VREG_CT
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	volatile uint  temp1[VREG1_CT]; 	for (int i = 0; i < VREG1_CT; i++) temp1[i] = i;
	volatile uint2 temp2[VREG2_CT]; 	for (int i = 0; i < VREG2_CT; i++) temp2[i] = i;
	volatile uint3 temp4[VREG4_CT]; 	for (int i = 0; i < VREG4_CT; i++) temp4[i] = i;
	volatile long4 temp8[VREG8_CT]; 	for (int i = 0; i < VREG8_CT; i++) temp8[i] = i;
		
	x.u ^= temp1[x.u] + temp2[x.u].x + temp4[x.u].x + temp8[x.u].x;	 	 


	// Process SREGS
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
if(SREG_CT>0)while(x.u+1)
if(SREG_CT>1)while(x.u+2)
if(SREG_CT>2)while(x.u+3)
if(SREG_CT>3)while(x.u+4)
if(SREG_CT>4)while(x.u+6)
 x.u=rsqrt(x.f);


	// Add to kernel's overall size
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
	if(SIZE_CT>100)
			{
		#pragma unroll 
		for(long i=0; i<SIZE_CT-100; i++)
			x.f = rsqrt(x.f);
			}
			
	 if (x.u < 0xFFFFFFF0) return; // prevents hang in case we accidentally run this				

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

	//x.fp[0x0000FFFF] = x.f;
}

__kernel void myAsmFunc(__global float* f0, __global uint* i1, __global uint* i2 )
{ 
	float input0 = *f0;
	uint input1 = *i1;
	uint input2 = *i2 +111;
	
	myAsmKernel000(input0, input1, input2);
}
