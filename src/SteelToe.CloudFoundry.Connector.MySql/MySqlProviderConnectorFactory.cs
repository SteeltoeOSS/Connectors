﻿//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using MySql.Data.MySqlClient;
using SteelToe.CloudFoundry.Connector.Services;
using System;

namespace SteelToe.CloudFoundry.Connector.MySql
{
    public class MySqlProviderConnectorFactory
    {
        private MySqlServiceInfo _info;
        private MySqlProviderConfiguration _config;
        private MySqlProviderConfigurer _configurer = new MySqlProviderConfigurer();
        public MySqlProviderConnectorFactory(MySqlServiceInfo sinfo, MySqlProviderConfiguration config)
        {
            _info = sinfo;
            _config = config;
        }
        internal MySqlConnection Create(IServiceProvider provider)
        {
            var connectionString = _configurer.Configure(_info, _config);
            return new MySqlConnection(connectionString);
        }
    }
}
