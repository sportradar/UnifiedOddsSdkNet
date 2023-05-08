/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class DemoMethods : MarshalByRefObject
    {
        [Log(LoggerType.Cache)]
        public void DemoVoidMethod()
        {
        }

        public int DemoIntMethod(int a, int b)
        {
            return a * b;
        }

        public int DemoLongLastingMethodAsyncCaller(int seed, int steps)
        {
            return DemoLongLastingMethodAsync(seed, steps).GetAwaiter().GetResult();
        }

        public async Task<int> DemoLongLastingMethodAsync(int seed, int steps)
        {
            return await Task.Run(() =>
                {
                    while (steps > 0)
                    {
                        seed += steps;
                        steps--;
                        Task.Delay(20).GetAwaiter().GetResult();
                    }

                    return seed;
                }
            );
        }

        public void DemoLongLastingVoidMethod(int sleep)
        {
            Task.Delay(sleep).GetAwaiter().GetResult();
        }

        public async Task<int> DemoLongLastingSleepMethodAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Task.Delay(sleep).GetAwaiter().GetResult();
                    return sleep;
                }
            );
        }

        public async Task<int> DemoMethodWithTaskThrowsExceptionAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Task.Delay(sleep).GetAwaiter().GetResult();
                    throw new Exception("DemoMethodWithTaskThrowsExceptionAsync exception.");
#pragma warning disable 162
                    return sleep;
#pragma warning restore 162
                }
            );
        }

        [Log(LoggerType.RestTraffic)]
        public async Task<DemoMethods> DemoCustomMethodAsync(int sleep)
        {
            return await Task.Run(() =>
                {
                    Task.Delay(sleep).GetAwaiter().GetResult();
                    return new DemoMethods();
                }
            );
        }

        public override string ToString()
        {
            return SdkInfo.GetRandom().ToString();
        }
    }
}
