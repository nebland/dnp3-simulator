﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;

using Automatak.Simulator.Commons;
using Automatak.Simulator.API;
using Automatak.Simulator.DNP3.API;

namespace Automatak.Simulator.DNP3
{
    static class IconIndex
    {
        public const int Channel = 0;                
        public const int Master = 1;
        public const int Outstation = 2;
    };


    class DNP3SimulatorPlugin : ISimulatorPlugin
    {
        readonly ImageList imgList = new ImageList();
        readonly ILogHandler logHandler;
        readonly IDNP3Manager manager;
        
        readonly DNP3Config config = new DNP3Config(
           new IOutstationModule[]{ 
               Automatak.Simulator.DNP3.DefaultOutstationPlugin.OutstationModule.Instance,
               Automatak.Simulator.DNP3.RelayOutstationPlugin.OutstationModule.Instance,
               Automatak.Simulator.DNP3.DerOutstationPlugin.OutstationModule.Instance
           }
        );

        public DNP3SimulatorPlugin(ILog log)
        {
            this.logHandler = new ForwardingLogHandler(log);

            this.manager = DNP3ManagerFactory.CreateManager(this.logHandler);

            imgList.Images.Add(Properties.Resources.satellite_dish);            
            imgList.Images.Add(Properties.Resources.network_monitor);
            imgList.Images.Add(Properties.Resources.send_container);

            /*
                Load outstation plugins here 
            */
        }

        string ISimulatorPlugin.UniqueId
        {
            get { return "DNP3"; }
        }

        System.Drawing.Image ISimulatorPlugin.PluginImage
        {
            get { return Properties.Resources.satellite_dish_add; }
        }


        string ISimulatorPlugin.RootDisplayName
        {
            get { return "DNP3 Channel"; }
        }


        ISimulatorNode ISimulatorPlugin.Create(ISimulatorNodeCallbacks callbacks)
        {
            using (var dialog = new Components.ChannelDialog())
            {
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var channel = dialog.ChannelAction.Invoke(manager);
                    return new ChannelNode(config, channel, callbacks, dialog.SelectedAlias);
                }
                else
                {
                    return null;    
                }
            }            
       }

        ISimulatorNode ISimulatorPlugin.CreateNoDialog(ISimulatorNodeCallbacks callbacks)
        {
            uint flags = LogLevels.ALL; //logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(new TimeSpan(0,0,1), new TimeSpan(0,0,10));

            IChannel channel = manager.AddTCPServer("channel", flags, retry, "127.0.0.1", 20000);

            return new ChannelNode(config, channel, callbacks, "channel");
        }

        System.Windows.Forms.ImageList ISimulatorPlugin.NodeImageList
       {
           get { return imgList; }
       }
    }
}
