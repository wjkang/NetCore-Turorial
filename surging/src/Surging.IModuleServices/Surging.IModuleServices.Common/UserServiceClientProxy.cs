//这是动态生成的代理服务实现，不属于surging源码

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.ProxyGenerator.Implementation;
namespace Surging.Cores.ClientProxys
{
    public class UserServiceClientProxy : ServiceProxyBase, Surging.IModuleServices.Common.IUserService
    {
        public UserServiceClientProxy(
            IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService,
            String serviceKey, CPlatformContainer serviceProvider) :
            base(remoteInvokeService, typeConvertibleService, serviceKey, serviceProvider)
        {
        }
        public async Task<Surging.IModuleServices.Common.Models.UserModel> Authentication(Surging.IModuleServices.Common.Models.AuthenticationRequestData requestData)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.UserModel>(new Dictionary<string, object>
            {
                {
                    "requestData", requestData
                }
            }
            , "Surging.IModuleServices.Common.IUserService.Authentication_requestData");
        }
        public async Task<System.String> GetUserName(System.Int32 id)
        {
            return await Invoke<System.String>(new Dictionary<string, object>
            {
                {
                    "id", id
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserName_id");
        }
        public async Task<System.Boolean> Exists(System.Int32 id)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "id", id } }, "Surging.IModuleServices.Common.IUserService.Exists_id");
        }
        public async Task<Surging.IModuleServices.Common.Models.IdentityUser> Save(Surging.IModuleServices.Common.Models.IdentityUser requestData)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.IdentityUser>(new Dictionary<string, object>
            {
                {
                    "requestData", requestData
                }
            }
            , "Surging.IModuleServices.Common.IUserService.Save_requestData");
        }
        public async Task<System.Int32> GetUserId(System.String userName)
        {
            return await Invoke<System.Int32>(new Dictionary<string, object>
            {
                {
                    "userName", userName
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserId_userName");
        }
        public async Task<System.DateTime> GetUserLastSignInTime(System.Int32 id)
        {
            return await Invoke<System.DateTime>(new Dictionary<string, object>
            {
                {
                    "id", id
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserLastSignInTime_id");
        }
        public async Task<Surging.IModuleServices.Common.Models.UserModel> GetUser(Surging.IModuleServices.Common.Models.UserModel user)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.UserModel>(new Dictionary<string, object>
            {
                {
                    "user", user
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUser_user");
        }
        public async Task<System.Boolean> Update(System.Int32 id, Surging.IModuleServices.Common.Models.UserModel model)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "id", id }, { "model", model } }, "Surging.IModuleServices.Common.IUserService.Update_id_model");
        }
        public async Task<System.Boolean> Get(List<Surging.IModuleServices.Common.Models.UserModel> users)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "users", users } }, "Surging.IModuleServices.Common.IUserService.Get_users");
        }
        public async Task<System.Boolean> GetDictionary()
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { }, "Surging.IModuleServices.Common.IUserService.GetDictionary");
        }
        public async System.Threading.Tasks.Task TryThrowException()
        {
            await Invoke(new Dictionary<string, object>
            {
            }
            , "Surging.IModuleServices.Common.IUserService.TryThrowException");
        }
        public async System.Threading.Tasks.Task PublishThroughEventBusAsync(Surging.Core.CPlatform.EventBus.Events.IntegrationEvent evt)
        {
            await Invoke(new Dictionary<string, object>
            {
                {
                    "evt", evt
                }
            }
            , "Surging.IModuleServices.Common.IUserService.PublishThroughEventBusAsync_evt");
        }
    }
}