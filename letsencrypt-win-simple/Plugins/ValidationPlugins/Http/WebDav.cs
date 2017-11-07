﻿using ACMESharp;
using LetsEncrypt.ACME.Simple.Client;
using LetsEncrypt.ACME.Simple.Configuration;
using LetsEncrypt.ACME.Simple.Services;
using System;
using System.Linq;

namespace LetsEncrypt.ACME.Simple.Plugins.ValidationPlugins.Http
{
    class WebDavFactory : IValidationPluginFactory
    {
        public string Name => nameof(WebDav);
        public string Description => "Upload verification file to WebDav path";
        public string ChallengeType => AcmeProtocol.CHALLENGE_TYPE_HTTP;
        public bool CanValidate(Target target) => string.IsNullOrEmpty(target.WebRootPath) || target.WebRootPath.StartsWith("\\\\");
        public Type Instance => typeof(WebDav);
    }

    class WebDav : HttpValidation
    {
        private WebDavClient _webdavClient;

        public WebDav(Target target, ILogService logService, IInputService inputService, IOptionsService optionsService) : base(logService, inputService, optionsService)
        {
            _webdavClient = new WebDavClient(target.HttpWebDavOptions);
        }

        public override void DeleteFile(string path)
        {
            _webdavClient.Delete(path);
        }

        public override void DeleteFolder(string path)
        {
            _webdavClient.Delete(path);
        }

        public override bool IsEmpty(string path)
        {
            return _webdavClient.GetFiles(path).Count() == 0;
        }

        public override void WriteFile(string path, string content)
        {
            _webdavClient.Upload(path, content);
        }

        public override void Default(Target target)
        {
            base.Default(target);
            if (string.IsNullOrEmpty(target.WebRootPath))
            {
                target.WebRootPath = _options.TryGetRequiredOption(nameof(_options.Options.WebRoot), _options.Options.WebRoot);
            }
            target.HttpWebDavOptions = new WebDavOptions(_options);
        }

        public override void Aquire(Target target)
        {
            base.Aquire(target);
            if (string.IsNullOrEmpty(target.WebRootPath))
            {
                target.WebRootPath = _options.TryGetOption(_options.Options.WebRoot, _input, new[] {
                     "Enter a site path (the web root of the host for http authentication)",
                    " Example, http://domain.com:80/",
                    " Example, https://domain.com:443/"
                });
            }
            target.HttpWebDavOptions = new WebDavOptions(_options, _input);
        }
    }
}
