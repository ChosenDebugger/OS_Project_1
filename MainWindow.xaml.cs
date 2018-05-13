using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;


//v2.0结构优化
//1.对已被分配但还没有完成处理的按钮进行分类：2->未分配; 1->已分配未完成; 0->无请求

namespace OS_Project_1
{
    //传信器：功能等同全局变量
    //2->request未被分配;1->request已被分配; 0 ->noRequest;
    //public class Messager {
    //    static string name = "123";
    //    static int []outRequest = new int[40]; }

    public partial class MainWindow : Window
    {
        public int currentElevator;                                         //当前框选电梯ID

        public DispatcherTimer timer = new DispatcherTimer();               //计时器

        public ElevatorController[] elevator = new ElevatorController[5];   //电梯控制器实例         

        public int[] outRequestStatus = new int[40];                              //外部按钮请求状态: 2表示未分配；1表示已分配但未完成；0表示已完成

        private int toAssign = 0;                                           //等待分配的请求数


        public MainWindow()
        {
            InitializeComponent();

            elevator[0] = new ElevatorController();                         //给每个电梯添加实例对象
            elevator[1] = new ElevatorController();
            elevator[2] = new ElevatorController();
            elevator[3] = new ElevatorController();
            elevator[4] = new ElevatorController();

            Thread ele1 = new Thread(new ParameterizedThreadStart(elevator[0].Run));
            Thread ele2 = new Thread(new ParameterizedThreadStart(elevator[1].Run));
            Thread ele3 = new Thread(new ParameterizedThreadStart(elevator[2].Run));
            Thread ele4 = new Thread(new ParameterizedThreadStart(elevator[3].Run));
            Thread ele5 = new Thread(new ParameterizedThreadStart(elevator[4].Run));
            Thread.CurrentThread.IsBackground = true;

            ele1.Name = @"ele1";
            ele2.Name = @"ele2";
            ele3.Name = @"ele3";
            ele4.Name = @"ele4";
            ele5.Name = @"ele5";

            ele1.Start(elevator1);
            ele2.Start(elevator2);
            ele3.Start(elevator3);
            ele4.Start(elevator4);
            ele5.Start(elevator5);

            //v1.5优化
            //合理安排按钮与电梯状态的切换
            //（声明多个计时器同时运作
            timer.Interval = TimeSpan.FromMilliseconds(100);                 //设定计时器时间间隔为500 ms

            timer.Start();
            timer.Tick += new EventHandler(UpdateInMessage);
            timer.Tick += new EventHandler(UpdateOutMessage);
            timer.Tick += new EventHandler(TargetAssign);
        }

        //v1.5 结构优化之后这里应根据请求状态变色
        //1.控制外部楼层灯
        //

        public void UpdateInMessage(object sender, EventArgs e)
        {
            //用于控制电梯内部按钮灯
            //需要依靠Elevator 内部变量inTarget 来判断
            //v1.0

            Label labelNum = FindName("Label1") as Label;

            labelNum.Content = (elevator[currentElevator].eControl.currentFloor).ToString();

            Label labelStatus = FindName("Label2") as Label;

            if (elevator[currentElevator].eControl.eStatus == 1) labelStatus.Content = "↑";
            if (elevator[currentElevator].eControl.eStatus == 0) labelStatus.Content = " - ";
            if (elevator[currentElevator].eControl.eStatus == -1) labelStatus.Content = "↓";

            for (int i = 0; i < 5; i++)
            {
                if (elevator[i].ifNormal == false) continue;

                if (elevator[i].eControl.eStatus == 0)
                    elevator[i].elevatorText.Background = Brushes.LightBlue;
                else
                    elevator[i].elevatorText.Background = Brushes.ForestGreen;
            }
            for (int i = 0; i < 20; i++)
            {
                string buttonName = @"button";
                buttonName += (i + 1).ToString();
                Button obj = FindName(buttonName) as Button;

                if (elevator[currentElevator].eControl.inLightStatus[i] == 1) { obj.Background = Brushes.OrangeRed; }
                if (elevator[currentElevator].eControl.inLightStatus[i] == 0) { obj.Background = Brushes.White; }
            }
        }

        public void UpdateOutMessage(object sender, EventArgs e)
        {
            //v1.0
            //用于控制外部的上下请求灯  √
            //需要依靠Elevator 内部变量outTarget 来判断

            //v2.0 优化1

            for (int i = 0; i < 20; i++)
            {
                string buttonName = @"button";
                buttonName += (i + 21).ToString();

                for (int j = 0; j < 5; j++)
                {
                    //Console.WriteLine("123123");
                    if (elevator[j].eControl.outTarget[0][i] == 1)
                    {
                        TurnPressButton(FindName(buttonName) as Button);
                        break;
                    }
                    TurnUnPressButton(FindName(buttonName) as Button);
                }
            }
            for (int i = 20; i < 40; i++)
            {
                string buttonName = @"button";
                buttonName += (i + 21).ToString();
                for (int j = 0; j < 5; j++)
                {
                    if (elevator[j].eControl.outTarget[1][i - 20] == 1)
                    {
                        TurnPressButton(FindName(buttonName) as Button);
                        break;
                    }
                    TurnUnPressButton(FindName(buttonName) as Button);
                }
            }
        }


        //按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应//
        //按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应////按键响应//

        private void Button_Click_inRequest(object sender, RoutedEventArgs e)
        {
            //电梯内部请求
            //更新inTarget状态√

            //if (!WhetherResponse(sender, e)) return;

            int buttonID = (sender as Button).TabIndex;

            elevator[currentElevator].eControl.inLightStatus[buttonID - 1] = 1;     //亮灯
            elevator[currentElevator].eControl.inTarget[buttonID - 1] = 1;

            elevator[currentElevator].eControl.toDeal++;
        }

        private void Button_Click_outRequest(object sender, RoutedEventArgs e)
        {
            //电梯外部请求
            //更新outTarget状态     √
            //待处理请求+1          √

            //if (!WhetherResponse(sender, e)) return;

            int buttonID = (sender as Button).TabIndex;

            outRequestStatus[buttonID - 1] = 2;
            toAssign++;
        }

        private void Button_Click_Error(object sender, RoutedEventArgs e)
        {
            elevator[currentElevator].elevatorText.Background = Brushes.DarkRed;
            elevator[currentElevator].ErrorCallback(FindName("LabelError") as Label);
        }
        private void Button_Click_Repair(object sender, RoutedEventArgs e)
        {
            elevator[currentElevator].RepairCallBack(FindName("LabelError") as Label);
            elevator[currentElevator].elevatorText.Background = Brushes.LightBlue;
        }

        private void Button_Click_Open(object sender, RoutedEventArgs e)
        {
            elevator[currentElevator].eControl.whetherWait = 1;
        }

        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            elevator[currentElevator].eControl.whetherWait = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //选择当前操作电梯
            currentElevator = whichElevator.SelectedIndex;
        }

        //v1.5 优化变色效果
        private void TurnPressButton(Button obj)
        {
            //按钮处于按下状态
            obj.Background = Brushes.Red;
        }

        private void TurnUnPressButton(Button obj)
        {
            //按钮处于松开状态
            obj.Background = Brushes.White;
        }


        //五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度
        //五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度//五部电梯调度

        //基于队列allRequest的任务调度分配器
        //当allRequest 不空时，就对其队头的请求按钮所表示的请求进行分配
        //完成button_click -> queue ->elevator[].target的转换
        //具体步骤：
        //Ⅰ判断表头请求的楼层及请求方向
        //Ⅱ根据请求方向安排电梯
        //  ①先找同向电梯 ->找出距离最短者
        //  ②若没有同向电梯 ->在静止电梯中找到距离最近
        //Ⅲ分配Target

        public void TargetAssign(object sender, EventArgs e)
        {
            if (toAssign == 0) return;

            int[] distance1 = new int[5];                           //顺路电梯距离请求的位置
            int[] distance2 = new int[5];                           //静止电梯距离请求的距离

            //inTarget
            //内部请求本身具有电梯指向性
            //因此不需要计算直接在Button响应中分配

            for (int i = 0; i < 40; i++)
            {
                //对状态为2的进行分配并将其status标为1
                if (outRequestStatus[i] != 2) continue;

                // buttonID = i + 1; 
                //i:0->19 UpRequest
                //i:20->39 DownRequest

                //upButton
                if (0 <= i && i <= 19)
                {
                    int floor = i + 1;                              //请求楼层数

                    for (int j = 0; j < 5; j++)
                    {
                        distance1[j] = 999;
                        distance2[j] = 999;
                    }

                    for (int j = 0; j < 5; j++)
                    {
                        //如果该电梯承担任务过多就调度下一部
                        if (elevator[j].eControl.eStatus == 1 && elevator[j].eControl.toDeal <= 5)
                        {
                            if (elevator[j].eControl.currentFloor <= floor)
                                distance1[j] = floor - elevator[j].eControl.currentFloor;
                        }
                        if (elevator[j].eControl.eStatus == 0)
                        {
                            distance2[j] = Math.Abs(floor - elevator[j].eControl.currentFloor);
                        }
                    }

                    int closestEleID = -1;                                  //最近的电梯ID
                    int minDistance = 20;                                   //最近的距离

                    for (int j = 0; j < 5; j++)
                        if (distance1[j] < minDistance)
                        {
                            minDistance = distance1[j];
                            closestEleID = j;
                        }

                    if (minDistance < 20)
                    {
                        elevator[closestEleID].eControl.outTarget[0][floor - 1] = 1;
                        elevator[closestEleID].eControl.toDeal++;
                        outRequestStatus[i] = 1;
                        toAssign--;
                        continue;
                    }

                    //如果没有顺路的电梯就找无任务静止的
                    minDistance = 20;
                    for (int j = 0; j < 5; j++)
                        if (distance2[j] < minDistance)
                        {
                            minDistance = distance2[j];
                            closestEleID = j;
                        }

                    if (minDistance < 20)
                    {
                        elevator[closestEleID].eControl.outTarget[0][floor - 1] = 1;
                        elevator[closestEleID].eControl.toDeal++;
                        outRequestStatus[i] = 1;
                        toAssign--;
                        continue;
                    }
                }

                //downButton
                if (20 <= i && i <= 39)
                {
                    int floor = i - 19;

                    for (int j = 0; j < 5; j++)
                    {
                        distance1[j] = 999;
                        distance2[j] = 999;
                    }

                    for (int j = 0; j < 5; j++)
                    {
                        if (elevator[j].eControl.eStatus == -1 && elevator[j].eControl.toDeal <= 5)
                        {
                            if (elevator[j].eControl.currentFloor >= floor)
                                distance1[j] = elevator[j].eControl.currentFloor - floor;
                        }
                        if (elevator[j].eControl.eStatus == 0)
                        {
                            distance2[j] = Math.Abs(floor - elevator[j].eControl.currentFloor);
                        }
                    }

                    int closestEleID = -1;                                  //最近的电梯ID
                    int minDistance = 20;                                   //最近的距离

                    for (int j = 0; j < 5; j++)
                        if (distance1[j] < minDistance)
                        {
                            minDistance = distance1[j];
                            closestEleID = j;
                        }

                    if (minDistance < 20)
                    {
                        elevator[closestEleID].eControl.outTarget[1][floor - 1] = 1;
                        elevator[closestEleID].eControl.toDeal++;
                        outRequestStatus[i] = 1;
                        toAssign--;
                        continue;
                    }

                    minDistance = 20;
                    for (int j = 0; j < 5; j++)
                        if (distance2[j] < minDistance)
                        {
                            minDistance = distance2[j];
                            closestEleID = j;
                        }
                    if (minDistance < 20)
                    {
                        elevator[closestEleID].eControl.outTarget[1][floor - 1] = 1;
                        elevator[closestEleID].eControl.toDeal++;
                        outRequestStatus[i] = 1;
                        toAssign--;
                        continue;
                    }
                }
            }
        }

        /*private bool WhetherResponse_In(object sender, RoutedEventArgs e)
        {
            //v1.5新增
            //判断是否要响应该指令
            //在Button 触发后第一时间被调用并进行判断
            return true;
        }*/

    } 
}
