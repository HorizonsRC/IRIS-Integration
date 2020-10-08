
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Web.Configuration;
using Conf = System.Configuration;
using System.IO;

namespace HRC.Common.Configuration
{
    public abstract class ConfigurationSectionEx : ConfigurationSection
    {
        #region Fields

        string m_SectionName = null;

        #endregion Variables

        #region Constructors

        #endregion Constructors

        #region Properties

        protected virtual bool IgnoreUnrecognizedAttributes
        {
            get
            {
                return true;
            }
        }

        protected virtual string SectionName
        {
            get
            {
                if (m_SectionName == null)
                {
                    m_SectionName = this.GetType().FullName.Replace('+', '_');
                }
                return m_SectionName;
            }
        }

        protected virtual bool AllowEmptySection
        {
            get
            {
                return false;
            }
        }

        protected virtual string OverrideConfigFilename
        {
            get
            {
                return "";
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Will perform a directory walk up the tree and look for <paramref name="filename"/>
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FindConfigFile(string filename)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(Assembly.GetEntryAssembly().Location).Parent;
            bool foundConfigPath = false;
            while (!foundConfigPath && currentDirectory != null)
            {
                foundConfigPath = File.Exists(Path.Combine(currentDirectory.FullName, filename));
                if (!foundConfigPath)
                {
                    currentDirectory = currentDirectory.Parent;
                }
            }
            return currentDirectory == null ? "" : Path.Combine(currentDirectory.FullName, filename);
        }

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            /*
             Need to handle this and return true so it doesnt throw an exception when we add attributes to the config file but havent updated all versions of HRC.Common yet.
            */
            return true;
        }

        protected virtual void Initialize()
        {
        }

        protected static T OpenConfig<T>(T template, string overrideFilename, bool suppressEmptySectionException, bool createEmptySection, out bool success) where T : ConfigurationSectionEx
        {
            Conf.Configuration conf;
            if (overrideFilename.SafeTrimOrEmpty() != "")
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = overrideFilename;
                conf = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            }
            else
            {
                try
                {
                    conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                catch (ArgumentException e)
                {
                    if (e.Message.Contains("exePath must be specified"))
                    {
                        conf = WebConfigurationManager.OpenWebConfiguration("~\\Web.config");
                    }
                    else
                    {
                        throw new Exception("Error Opening Config File", e);
                    }
                }
            }

            T section = (T)conf.Sections[template.SectionName];
            success = section != null;
            if (section == null)
            {
                if (createEmptySection)
                {
                    section = template;
                    conf.Sections.Add(template.SectionName, section);
                    section.SectionInformation.ForceSave = true;
                    conf.Save();
                }
                if (!template.AllowEmptySection && !suppressEmptySectionException)
                {
                    throw new ConfigurationErrorsException("Config not set");
                }
            }
            if (section != null)
            {
                section.Initialize();
            }
            return section;
        }


        protected static T OpenConfig<T>(T template) where T : ConfigurationSectionEx
        {
            bool temp;
            return OpenConfig<T>(template, template.OverrideConfigFilename, false, true, out temp);
        }

        protected virtual void SaveInternal()
        {
            Conf.Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSectionEx section = (ConfigurationSectionEx)conf.Sections[this.SectionName];
            PropertyInfo[] properties = section.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite && property.GetCustomAttributes(typeof(ConfigurationPropertyAttribute), true).Length==1)
                {
                    property.SetValue(section, property.GetValue(this, null), null);
                }
            }
            conf.Save();
        }

        public virtual void Save()
        {
            SaveInternal();
        }
        #endregion Methods
    }
}
