using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Project_1
{
    public class Elevator       //电梯数据块
    {
        public int eStatus;                                                     //电梯运行状况 0->stay; 1->up;-1->down  
        public int toDeal;                                                      //剩下的待处理请求
        public int whetherWait;                                                 //1表示停靠（开门）；2表示加速离开（关门）

        public int currentFloor;                                                //电梯当前楼层
        public int currentTarget;                                               //运行状态下当前的目标楼层，0表示无目标

        public int typeOfOutTarget;                                             //标识是对哪种outTarget的响应,0表示up；1表示down；-1表示无
        public int[] inTarget = new int[20];                                    //所有内部请求楼层标识，2->有请求且未被分配；1->有请求且已被分配；0->无请求
        public int[][] outTarget = new int[2][];                                //标识楼层Button请求，2->有请求且未被分配；1->有请求且已被分配；0->无请求

        public int[] inLightStatus = new int[20];                                //电梯内部的楼层状态灯

        public Elevator()
        {
            eStatus = 0;
            toDeal = 0;
            currentFloor = 1;
            currentTarget = 0;           //changes happen here
            typeOfOutTarget = -1;

            outTarget[0] = new int[20];
            outTarget[1] = new int[20];
        }
    }
}
