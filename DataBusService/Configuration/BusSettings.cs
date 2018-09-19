//Copyright 2018 Damir Garipov
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System.Configuration;

namespace DataBusService.Configuration
{
    [ConfigurationCollection(typeof(BusSettingsElement))]
    public class BusSettings: ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BusSettingsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BusSettingsElement) element).Key;
        }

        public BusSettingsElement this[int index] => (BusSettingsElement)BaseGet(index);

        public new BusSettingsElement this[string key] => (BusSettingsElement)BaseGet(key);
    }

    public class BusSettingsElement: ConfigurationElement
    {
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public string Key
        {
            get => (string)base["key"];
            set => base["key"] = value;
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get => (string) base["value"];
            set => base["value"] = value;
        }
    }
}
