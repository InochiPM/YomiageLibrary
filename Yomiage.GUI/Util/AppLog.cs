﻿using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yomiage.GUI.Util
{
    static class AppLog
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static AppLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = @"log\log" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".txt" };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
            logger.Info(" Start");
        }

        public static void Init()
        {

        }

        public static void CurrentDomain_FirstChanceException(
        object sender,
        System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Exception?.Message))
            {
                if (e.Exception.Message == "ウィンドウ ハンドルが無効です。") { return; }
                if (e.Exception.Message.Contains("JpnKanaConversion.XmlSerializers.dll")) { return; }
            }
            logger.Error("マネージコード内で例外が発生しました。");
            logger.Error(e.ToString());
            logger.Error("e.Exception.TargetSite.Name : " + e.Exception?.TargetSite?.Name);
            logger.Error("e.Exception.Message : " + e.Exception?.Message);
            logger.Error("e.Exception.StackTrace : " + e.Exception?.StackTrace);
            logger.Error("e.Exception.Source : " + e.Exception?.Source);
        }

        public static void App_DispatcherUnhandledException(
                object sender,
                System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error("UIスレッドで例外が発生しました。");
            logger.Error(e.ToString());
            logger.Error("e.Exception.TargetSite.Name : " + e.Exception?.TargetSite?.Name);
            logger.Error("e.Exception.Message : " + e.Exception?.Message);
            logger.Error("e.Exception.StackTrace : " + e.Exception?.StackTrace);
            logger.Error("e.Exception.Source : " + e.Exception?.Source);

            e.Handled = true;
        }

        public static void TaskScheduler_UnobservedTaskException(
                object sender,
                UnobservedTaskExceptionEventArgs e)
        {
            logger.Error("バックグラウンドで例外が発生しました。");
            logger.Error(e.ToString());
            logger.Error("e.Exception.TargetSite.Name : " + e.Exception?.TargetSite?.Name);
            logger.Error("e.Exception.Message : " + e.Exception?.Message);
            logger.Error("e.Exception.StackTrace : " + e.Exception?.StackTrace);
            logger.Error("e.Exception.Source : " + e.Exception?.Source);

            e.SetObserved();
        }

        public static void CurrentDomain_UnhandledException(
                  object sender,
                  UnhandledExceptionEventArgs e)
        {
            logger.Error("トラップできない例外が発生しました。");
            logger.Error(e.ToString());
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                logger.Error("System.Exceptionとして扱えない例外");
                return;
            }

            logger.Error("exception.TargetSite.Name : " + exception.TargetSite?.Name);
            logger.Error("exception.Message : " + exception.Message);
            logger.Error("exception.StackTrace : " + exception.StackTrace);
            logger.Error("exception.Source : " + exception.Source);


            Environment.Exit(0);
        }

    }
}