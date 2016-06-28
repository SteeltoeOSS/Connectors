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

using SteelToe.Extensions.Configuration.CloudFoundry;

namespace SteelToe.CloudFoundry.Connector.Services
{
    [ServiceInfoFactory]
    public class RedisServiceInfoFactory : ServiceInfoFactory
    {

        public RedisServiceInfoFactory() :
            base(new Tags("redis"), RedisServiceInfo.REDIS_SCHEME)
        {

        }

        public override IServiceInfo Create(Service binding)
        {
            string uri = GetUriFromCredentials(binding.Credentials);
            if (string.IsNullOrEmpty(uri))
            {
                string host = GetHostFromCredentials(binding.Credentials);
                int port = GetPortFromCredentials(binding.Credentials);
                string password = GetPasswordFromCredentials(binding.Credentials);

                return new RedisServiceInfo(binding.Name, host, port, password);
            } else
            {
                return new RedisServiceInfo(binding.Name, uri);
            }
        }
    }
}
