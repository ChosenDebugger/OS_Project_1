﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Media;
using OS_Project_1_v1;

namespace OS_Project_1
{
    //给每个电梯都设立独立的ElevatorController 类
    //用来判断电梯的当前状态，并设置接下来的状态

    public class ElevatorController
    {
        public bool ifNormal = true;

        public Elevator eControl = new Elevator();
        public TextBox elevatorText = new TextBox();
        
        //Based on the given TargetFloor, set the direction √
        public void SetDirection()
        {
            if (eControl.currentTarget == eControl.currentFloor) eControl.eStatus = 0;

            eControl.eStatus = (eControl.currentTarget > eControl.currentFloor) ? 1 : -1;
        }

        //main function to be invoked
        public void Run(object t)
        {
            elevatorText = (t as TextBox);

            while (true)
            {
                if (ifNormal == true)
                {
                    SetTarget();

                    if (WhetherStop())
                    {
                        this.eControl.eStatus = 0;
                        this.ElvatorStop();

                        while (eControl.whetherWait == 1)
                        {
                            eControl.whetherWait = 0;
                            Thread.Sleep(1000);
                        }
                        Thread.Sleep(200);
                    }
                    this.elevatorText.Dispatcher.Invoke(Move);

                    Thread.Sleep(1000);
                }
            }

        }

        public void SetTarget()
        {
            if (eControl.toDeal == 0) return;

            //向上状态分析
            //检测内外部请求中有没有必当前目标更近的楼层
            //如有则切换当前目标
            //没有则do nothing
            if (eControl.eStatus == 1)
            {
                for (int i = eControl.currentTarget - 2; i > eControl.currentFloor - 1; i--) 
                {
                    if (eControl.inTarget[i] == 1)
                    {
                        eControl.currentTarget = i + 1;
                        eControl.typeOfOutTarget = -1;
                        return; 
                    }
                    if (eControl.outTarget[0][i] == 1)
                    {
                        eControl.currentTarget = i + 1;
                        eControl.typeOfOutTarget = 0;
                        return;
                    }
                }
            }

            //向下 ->需求同上
            if (eControl.eStatus == -1) 
            {
                for (int i = eControl.currentTarget; i < eControl.currentFloor-2; i++)
                {
                    if (eControl.inTarget[i] == 1)
                    {
                        eControl.currentTarget = i + 1;
                        eControl.typeOfOutTarget = -1;
                        return;
                    }
                    if(eControl.outTarget[1][i] == 1)
                    {
                        eControl.currentTarget = i + 1;
                        eControl.typeOfOutTarget = 1;
                        return;
                    }
                }
            }

            //静止状态分析
            //检测最近的请求并将其设为当前目标楼层
            if (eControl.eStatus == 0) 
            {
                int distanceUp = 999, distanceDown = 999;

                for (int i = eControl.currentFloor - 1; i < 20; i++)
                {
                    if (eControl.inTarget[i] == 1 || eControl.outTarget[0][i] == 1 || eControl.outTarget[1][i] == 1)
                    {
                        distanceUp = i + 1 - eControl.currentFloor;
                        break;
                    }
                }
                for (int i = eControl.currentFloor - 2; i >= 0; i--)
                {
                    if (eControl.inTarget[i] == 1 || eControl.outTarget[0][i] == 1 || eControl.outTarget[1][i] == 1)
                    {
                        distanceDown = eControl.currentFloor - (i + 1);
                        break;
                    }
                }
                if (distanceUp <= distanceDown && distanceUp != 999) { eControl.currentTarget = eControl.currentFloor + distanceUp; }
                if (distanceUp > distanceDown) { eControl.currentTarget = eControl.currentFloor - distanceDown; }

                if (eControl.inTarget[eControl.currentTarget - 1] == 1)
                    eControl.typeOfOutTarget = -1;
                if (eControl.outTarget[0][eControl.currentTarget - 1] == 1)
                    eControl.typeOfOutTarget = 0;
                if (eControl.outTarget[1][eControl.currentTarget - 1] == 1)
                    eControl.typeOfOutTarget = 1;
            }
            SetDirection();
        }

        //只要根据当前的运动状态做出TextBox的行为回应即可
        //v1.0
        public void Move()
        {
            if (eControl.eStatus == 0) return;

            double left = elevatorText.Margin.Left;
            double bottom = elevatorText.Margin.Bottom;

            if (eControl.eStatus == 1)
            {
                bottom += 30;
                eControl.currentFloor++;
            }
            else if (eControl.eStatus == -1)
            {
                bottom -= 30;
                eControl.currentFloor--;
            }
            elevatorText.Margin = new Thickness(left, 0, 0, bottom);
        }


        //仅仅用于判断是否需要停靠      √
        private bool WhetherStop() { return eControl.currentFloor == eControl.currentTarget; }

        //根据当前楼层和当前目标楼层判断是否需要停靠
        //只要相等就停靠
        //待处理请求-1
        public void ElvatorStop()
        {
            int floor = eControl.currentFloor;

            if (eControl.inTarget[floor - 1] == 1) { eControl.inTarget[floor - 1] = 0; }

            if (eControl.typeOfOutTarget == 0 || eControl.typeOfOutTarget == 1)
                if (eControl.outTarget[eControl.typeOfOutTarget][floor - 1] == 1)
                    eControl.outTarget[eControl.typeOfOutTarget][floor - 1] = 0;
            
            eControl.inLightStatus[floor - 1] = 0;
            eControl.toDeal--;
            eControl.currentTarget = -1;

        }

        public void ErrorCallback(Label errorLabel)
        {
            errorLabel.Content = ("Elevator " + elevatorText.TabIndex + "Error!");
            errorLabel.FontSize = 16;
            errorLabel.Background = Brushes.DarkRed;

            elevatorText.Background = Brushes.Red;

            ifNormal = false;
        }

        public void RepairCallBack(Label errorLabel)
        {
            errorLabel.Content = "";
            errorLabel.Background = Brushes.Transparent;

 

            ifNormal = true;
        }

        public bool DoorOpenCallBack() { return true; }

        public bool DoorCloseCallBack() { return true; }

    }
}
