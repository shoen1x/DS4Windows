﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DS4Windows.InputDevices;
using DS4WinWPF.DS4Control.DTOXml;

namespace DS4Windows
{
    public class ControlServiceDeviceOptions
    {
        private DS4DeviceOptions dS4DeviceOpts = new DS4DeviceOptions();
        public DS4DeviceOptions DS4DeviceOpts { get => dS4DeviceOpts; }

        private DualSenseDeviceOptions dualSenseOpts = new DualSenseDeviceOptions();
        public DualSenseDeviceOptions DualSenseOpts { get => dualSenseOpts; }

        private SwitchProDeviceOptions switchProDeviceOpts = new SwitchProDeviceOptions();
        public SwitchProDeviceOptions SwitchProDeviceOpts { get => switchProDeviceOpts; }

        private JoyConDeviceOptions joyConDeviceOpts = new JoyConDeviceOptions();
        public JoyConDeviceOptions JoyConDeviceOpts { get => joyConDeviceOpts; }

        private bool verboseLogMessages;
        public bool VerboseLogMessages { get => verboseLogMessages; set => verboseLogMessages = value; }

        public ControlServiceDeviceOptions()
        {
            // If enabled then DS4Windows shows additional log messages when a gamepad is connected (may be useful to diagnose connection problems).
            // This option is not persistent (ie. not saved into config files), so if enabled then it is reset back to FALSE when DS4Windows is restarted.
            verboseLogMessages = false;
        }
    }

    public abstract class ControllerOptionsStore
    {
        protected InputDeviceType deviceType;
        public InputDeviceType DeviceType { get => deviceType; }

        public ControllerOptionsStore(InputDeviceType deviceType)
        {
            this.deviceType = deviceType;
        }

        public virtual void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }

        public virtual void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
        }
    }

    public class DS4DeviceOptions
    {
        public const bool DEFAULT_ENABLE = true;
        private bool enabled = DEFAULT_ENABLE;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class DS4ControllerOptions : ControllerOptionsStore
    {
        public const string XML_ELEMENT_NAME = "DS4SupportSettings";
        private bool copyCatController;
        public bool IsCopyCat
        {
            get => copyCatController;
            set
            {
                if (copyCatController == value) return;
                copyCatController = value;
                IsCopyCatChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsCopyCatChanged;

        public DS4ControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            string testStr = string.Empty;
            XmlSerializer serializer = new XmlSerializer(typeof(DS4ControllerOptsDTO));

            using (StringWriter strWriter = new StringWriter())
            {
                using XmlWriter xmlWriter = XmlWriter.Create(strWriter,
                    new XmlWriterSettings()
                    {
                        Encoding = Encoding.UTF8,
                        Indent = false,
                        OmitXmlDeclaration = true, // only partial XML with no declaration
                    });

                // Write root element and children
                DS4ControllerOptsDTO dto = new DS4ControllerOptsDTO();
                dto.MapFrom(this);
                // Omit xmlns:xsi and xmlns:xsd from output
                serializer.Serialize(xmlWriter, dto,
                    new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                xmlWriter.Flush();
                xmlWriter.Close();

                testStr = strWriter.ToString();
                //Trace.WriteLine("TEST OUTPUT");
                //Trace.WriteLine(testStr);
            }

            XmlNode tempDS4Node = xmlDoc.CreateDocumentFragment();
            tempDS4Node.InnerXml = testStr;

            XmlNode tempOptsNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (tempOptsNode != null)
            {
                node.RemoveChild(tempOptsNode);
            }

            tempOptsNode = tempDS4Node;
            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DS4ControllerOptsDTO));
            XmlNode baseNode = node.SelectSingleNode(XML_ELEMENT_NAME);
            if (baseNode == null)
                return;

            try
            {
                using var stringReader = new StringReader(baseNode.OuterXml);
                using var xmlReader = XmlReader.Create(stringReader);
                DS4ControllerOptsDTO dto = serializer.Deserialize(xmlReader) as DS4ControllerOptsDTO;
                dto.MapTo(this);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }

    public class DualSenseDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class DualSenseControllerOptions : ControllerOptionsStore
    {
        public enum LEDBarMode : ushort
        {
            Off,
            MultipleControllers,
            BatteryPercentage,
            On,
        }

        public enum MuteLEDMode : ushort
        {
            Off,
            On,
            Pulse
        }

        private bool enableRumble = true;
        public bool EnableRumble
        {
            get => enableRumble;
            set
            {
                if (enableRumble == value) return;
                enableRumble = value;
                EnableRumbleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableRumbleChanged;

        private DualSenseDevice.HapticIntensity hapticIntensity = DualSenseDevice.HapticIntensity.Medium;
        public DualSenseDevice.HapticIntensity HapticIntensity
        {
            get => hapticIntensity;
            set
            {
                if (hapticIntensity == value) return;
                hapticIntensity = value;
                HapticIntensityChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler HapticIntensityChanged;

        private LEDBarMode ledMode = LEDBarMode.MultipleControllers;
        public LEDBarMode LedMode
        {
            get => ledMode;
            set
            {
                if (ledMode == value) return;
                ledMode = value;
                LedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler LedModeChanged;

        private MuteLEDMode muteLedMode = MuteLEDMode.Off;
        public MuteLEDMode MuteLedMode
        {
            get => muteLedMode;
            set
            {
                if (muteLedMode == value) return;
                muteLedMode = value;
                MuteLedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler MuteLedModeChanged;

        public DualSenseControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode tempOptsNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("DualSenseSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempRumbleNode = xmlDoc.CreateElement("EnableRumble");
            tempRumbleNode.InnerText = enableRumble.ToString();
            tempOptsNode.AppendChild(tempRumbleNode);

            XmlNode tempRumbleStrengthNode = xmlDoc.CreateElement("RumbleStrength");
            tempRumbleStrengthNode.InnerText = hapticIntensity.ToString();
            tempOptsNode.AppendChild(tempRumbleStrengthNode);

            XmlNode tempLedMode = xmlDoc.CreateElement("LEDBarMode");
            tempLedMode.InnerText = ledMode.ToString();
            tempOptsNode.AppendChild(tempLedMode);

            XmlNode tempMuteLedMode = xmlDoc.CreateElement("MuteLEDMode");
            tempMuteLedMode.InnerText = muteLedMode.ToString();
            tempOptsNode.AppendChild(tempMuteLedMode);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("DualSenseSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableRumble");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableRumble = temp;
                }

                XmlNode itemStrength = baseNode.SelectSingleNode("RumbleStrength");
                if (Enum.TryParse(itemStrength?.InnerText ?? "",
                    out DualSenseDevice.HapticIntensity tempHap))
                {
                    hapticIntensity = tempHap;
                }

                XmlNode itemLedMode = baseNode.SelectSingleNode("LEDBarMode");
                if (Enum.TryParse(itemLedMode?.InnerText ?? "",
                    out LEDBarMode tempLED))
                {
                    ledMode = tempLED;
                }

                XmlNode itemMuteLedMode = baseNode.SelectSingleNode("MuteLEDMode");
                if (Enum.TryParse(itemMuteLedMode?.InnerText ?? "",
                    out MuteLEDMode tempMuteLED))
                {
                    muteLedMode = tempMuteLED;
                }
            }
        }
    }

    public class SwitchProDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;
    }

    public class SwitchProControllerOptions : ControllerOptionsStore
    {
        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public SwitchProControllerOptions(InputDeviceType deviceType) : base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode tempOptsNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("SwitchProSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("SwitchProSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableHomeLED = temp;
                }
            }
        }
    }

    public class JoyConDeviceOptions
    {
        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;
                enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnabledChanged;

        public enum LinkMode : ushort
        {
            Split,
            Joined,
        }

        private LinkMode linkedMode = LinkMode.Joined;
        public LinkMode LinkedMode
        {
            get => linkedMode;
            set
            {
                if (linkedMode == value) return;
                linkedMode = value;
            }
        }

        public enum JoinedGyroProvider : ushort
        {
            JoyConL,
            JoyConR,
        }

        private JoinedGyroProvider joinGyroProv = JoinedGyroProvider.JoyConR;
        public JoinedGyroProvider JoinGyroProv
        {
            get => joinGyroProv;
            set
            {
                if (joinGyroProv == value) return;
                joinGyroProv = value;
            }
        }
    }

    public class JoyConControllerOptions : ControllerOptionsStore
    {
        private bool enableHomeLED = true;
        public bool EnableHomeLED
        {
            get => enableHomeLED;
            set
            {
                if (enableHomeLED == value) return;
                enableHomeLED = value;
                EnableHomeLEDChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler EnableHomeLEDChanged;

        public JoyConControllerOptions(InputDeviceType deviceType) :
            base(deviceType)
        {
        }

        public override void PersistSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode tempOptsNode = node.SelectSingleNode("JoyConSupportSettings");
            if (tempOptsNode == null)
            {
                tempOptsNode = xmlDoc.CreateElement("JoyConSupportSettings");
            }
            else
            {
                tempOptsNode.RemoveAll();
            }

            XmlNode tempElement = xmlDoc.CreateElement("EnableHomeLED");
            tempElement.InnerText = enableHomeLED.ToString();
            tempOptsNode.AppendChild(tempElement);

            node.AppendChild(tempOptsNode);
        }

        public override void LoadSettings(XmlDocument xmlDoc, XmlNode node)
        {
            XmlNode baseNode = node.SelectSingleNode("JoyConSupportSettings");
            if (baseNode != null)
            {
                XmlNode item = baseNode.SelectSingleNode("EnableHomeLED");
                if (bool.TryParse(item?.InnerText ?? "", out bool temp))
                {
                    enableHomeLED = temp;
                }
            }
        }
    }
}
