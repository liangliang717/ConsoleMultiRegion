using System.Diagnostics;

namespace ConsoleMultiRegion
{
    class ListSelection
    {
        public static void Run(string[] args)
        {
            // 准备列表数据
            string[] options = {
                "1. 启动数据库",
                "2. 停止数据库",
                "3. 重启数据库",
                "4. 查看运行状态",
                "5. 退出程序"
            };

            string currentUser = Environment.UserName;
            const string dbUser = "gbasedbt";

            Console.WriteLine($"=== gbase8s运维控制台({currentUser}) ===");
            Console.WriteLine("请使用 ↑/↓ 方向键选择，按 Enter 确认：\n");

            // 调用自定义列表选择方法
            int selectedIndex = ShowSelectionMenu(options);
            // 清理屏幕并输出最终结果

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"您最终选择了: {options[selectedIndex]}");
            Console.ResetColor();

            switch (selectedIndex)
            {
                case 0:

                    if(currentUser != dbUser)
                    {
                        ExecuteCommandWithRealTimeOutput($"su - {dbUser} -c 'oninit -vy'");
                    }
                    else
                    {
                        ExecuteCommandWithRealTimeOutput("oninit -vy");
                    }

                    break;
                case 1:
                    if(currentUser != dbUser)
                    {
                        ExecuteCommandWithRealTimeOutput($"su - {dbUser} -c 'onmode -ky'");
                    }
                    else
                    {
                        ExecuteCommandWithRealTimeOutput("onmode -ky"); 
                    }
                    break;
                case 2:
                    if(currentUser != dbUser)
                    {
                        ExecuteCommandWithRealTimeOutput($"su - {dbUser} -c 'onmode -ky'");
                        ExecuteCommandWithRealTimeOutput($"sleep 10");
                        ExecuteCommandWithRealTimeOutput($"su - {dbUser} -c 'oninit -vy'");
                    }
                    else
                    {
                        ExecuteCommandWithRealTimeOutput("onmode -ky");
                        ExecuteCommandWithRealTimeOutput("sleep 10");
                        ExecuteCommandWithRealTimeOutput("oninit -vy");
                    }
                    break; 
                case 3:
                    if(currentUser != dbUser)
                    {
                        ExecuteCommandWithRealTimeOutput($"su - {dbUser} -c 'onstat -g ntt'");
                    }
                    else
                    {
                        ExecuteCommandWithRealTimeOutput("onstat -g ntt");
                    }
                    break;
                //case 3:
                //    ExecuteCommandWithRealTimeOutput("sudo sync && sudo echo 3 > /proc/sys/vm/drop_caches");
                //    break;
                //case 4:
                //    ExecuteCommandWithRealTimeOutput("tail -f /var/log/syslog");
                //    break;
                case 4:
                    Console.WriteLine("已退出程序");
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                    return;
            }


            // 调用自定义的选择方法
            bool isConfirmed = ShowConfirmation("是否退出该应用程序？");

           
            if (isConfirmed)
            {
                Console.WriteLine("已退出程序");
                Console.CursorVisible = true;
                Environment.Exit(0);
            }
            else
            {
                Console.Clear();
                Run(args);
            }


        }

        /// <summary>
        /// 显示交互式列表菜单
        /// </summary>
        static int ShowSelectionMenu(string[] options)
        {
            // 1. 隐藏光标，防止光标在重绘时乱闪
            Console.CursorVisible = false;

            int selectedIndex = 0;
            // 记录列表开始绘制的 Y 轴坐标（行号）
            int startY = Console.CursorTop;

            while (true)
            {
                // 2. 重绘整个列表
                for (int i = 0; i < options.Length; i++)
                {
                    // 将光标移动到当前选项所在的行首
                    Console.SetCursorPosition(0, startY + i);

                    // 【关键】清空当前行：防止上一次的高亮背景色或长文本残留
                    Console.ResetColor();
                    // 用空格填满当前行（减去1是为了防止换行导致屏幕滚动）
                    Console.Write(new string(' ', Console.WindowWidth - 1));

                    // 重新回到行首准备写字
                    Console.SetCursorPosition(0, startY + i);

                    // 3. 根据是否选中，设置不同的样式
                    if (i == selectedIndex)
                    {
                        // 选中状态：黑字白底，前面加个 ">" 箭头
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($" > {options[i]} ");
                    }
                    else
                    {
                        // 未选中状态：默认颜色，前面留空对齐
                        Console.Write($"   {options[i]} ");
                    }

                    // 恢复默认颜色
                    Console.ResetColor();
                }

                // 4. 捕获键盘输入 (true 表示不在屏幕上打印按键字符)
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // 5. 处理按键逻辑
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    // 如果到了顶部，循环到底部
                    if (selectedIndex < 0) selectedIndex = options.Length - 1;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    // 如果到了底部，循环到顶部
                    if (selectedIndex >= options.Length) selectedIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    // 恢复光标显示
                    Console.CursorVisible = true;
                    // 将光标移动到列表下方，避免后续输出覆盖菜单
                    Console.SetCursorPosition(0, startY + options.Length + 1);
                    return selectedIndex; // 返回选中的索引
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    // 可选：按 Esc 取消退出
                    Console.CursorVisible = true;
                    Environment.Exit(0);
                }
            }
        }

        static void ExecuteCommandWithRealTimeOutput(string command)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            // 订阅输出事件
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    Console.WriteLine("[输出] " + args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    Console.WriteLine("[错误] " + args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        /// <summary>
        /// 自定义交互式确认框
        /// </summary>
        static bool ShowConfirmation(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("(使用 Tab/方向键 切换，Enter 确认，Y/N 快捷选择)");

            int selectedIndex = 0; // 0: 是, 1: 否
            int startY = Console.CursorTop; // 记录选项所在行的 Y 坐标，用于反复重绘

            while (true)
            {
                // 1. 移动光标到选项行，并清空该行（防止残留字符）
                Console.SetCursorPosition(0, startY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, startY);

                // 2. 绘制 "[ 是 ]"
                if (selectedIndex == 0) SetHighlightStyle();
                Console.Write("[ 是 ]");
                Console.ResetColor();

                Console.Write("    "); // 选项间距

                // 3. 绘制 "[ 否 ]"
                if (selectedIndex == 1) SetHighlightStyle();
                Console.Write("[ 否 ]");
                Console.ResetColor();

                // 4. 监听键盘输入 (true 表示不在屏幕上打印按下的键)
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // 5. 处理交互逻辑
                if (keyInfo.Key == ConsoleKey.LeftArrow || (keyInfo.Key == ConsoleKey.Tab && keyInfo.Modifiers == ConsoleModifiers.Shift))
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = 1;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.Tab)
                {
                    selectedIndex++;
                    if (selectedIndex > 1) selectedIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("\n");
                    return selectedIndex == 0;
                }
                // 快捷键支持
                else if (keyInfo.Key == ConsoleKey.Y)
                {
                    Console.WriteLine("\n");
                    return true;
                }
                else if (keyInfo.Key == ConsoleKey.N)
                {
                    Console.WriteLine("\n");
                    return false;
                }
            }
        }

        // 设置高亮样式（黑底白字）
        static void SetHighlightStyle()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }

    }
}
