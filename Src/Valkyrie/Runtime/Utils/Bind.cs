using System;
using System.Reflection;
using System.Threading;
using Valkyrie.Di;

namespace Utils
{
    public class Bind
    {
        //TODO: Add two-way binding

        private bool _bindingBuild;

        private Func<object> _sourceFunc;
        private object _source => _sourceFunc();
        private string _path;
        private object _target;
        private string _targetPath;

        private string _updatedEventName;

        private Func<object> _getValue;
        private Action<object> _setValue;
        
        private Action _releaseOldSourceBinding;

        IBindingAdapter _sourceConverter;
        
        #region Source

        public Func<object> Source
        {
            get { return _sourceFunc; }
            set
            {
                if(value == _sourceFunc)
                    return;
                _sourceFunc = value;
                BuildSource();
                BuildSourceEvents();
                InnerUpdate();
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                if(_path == value)
                    return;
                _path = value;
                BuildSource();
                InnerUpdate();
            }
        }
        public string UpdatedEventName
        {
            get { return _updatedEventName; }
            set
            {
                if(value == null)
                    return;
                _updatedEventName = value;
                BuildSourceEvents();
                InnerUpdate();
            }
        }

        #endregion

        #region Target

        public object Target
        {
            get { return _target; }
            set
            {
                if(_target == value)
                    return;
                _target = value;
                BuildTarget();
                InnerUpdate();
            }
        }
        public string TargetPath
        {
            get { return _targetPath; }
            set
            {
                if(_targetPath == value)
                    return;
                _targetPath = value;
                BuildTarget();
                InnerUpdate();
            }
        }

        #endregion

        #region Value convertings

        public IBindingAdapter SourceConverter
        {
            get { return _sourceConverter; }
            set
            {
                if (_sourceConverter == value)
                    return;
                _sourceConverter = value;
                BuildSource();
                BuildSourceEvents();
                InnerUpdate();
            }
        }

        #endregion

        public bool AllowPrivateProperties { get; set; } = false;
        
        public event Action Updated;

        #region Private methods

        void BuildSource()
        {
            _getValue = null;
            if (_source == null)
                return;
            if(string.IsNullOrEmpty(_path))
                return;

            var sourceType = _source.GetType();
            var propertyInfo = AllowPrivateProperties
                ? sourceType.GetProperty(_path, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                : sourceType.GetProperty(_path);
            
            if (_sourceConverter != null)
                _getValue = () => _sourceConverter.Convert(propertyInfo.GetValue(_source, null));
            else
                _getValue = () => propertyInfo.GetValue(_source, null);
            _setSourceValue = (v) => propertyInfo.SetValue(_source, v, null);
        }
        void BuildSourceEvents()
        {
            if (_releaseOldSourceBinding != null)
            {
                _releaseOldSourceBinding();
                _releaseOldSourceBinding = null;
            }

            if (_source == null)
                return;
            if (string.IsNullOrEmpty(_updatedEventName))
                return;

            var eventInfo = _source.GetType().GetEvent(_updatedEventName);

            Action actionOnChange = OnSourceValueChange;
            Delegate delegateOnChange = actionOnChange;

            eventInfo.AddEventHandler(_source, delegateOnChange);
            _releaseOldSourceBinding = () =>
            {
                eventInfo.RemoveEventHandler(_source, delegateOnChange);
            };
        }

        void BuildTarget()
        {
            _setValue = null;
            if(_target == null)
                return;
            if(string.IsNullOrEmpty(_targetPath))
                return;

            var targetType = _target.GetType();
            var propertyInfo = AllowPrivateProperties
                ? targetType.GetProperty(_targetPath, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                : targetType.GetProperty(_targetPath);
            _setValue = value => propertyInfo.SetValue(_target, value, null);

            _getTargetValue = () => propertyInfo.GetValue(_target, null);
        }

        void OnSourceValueChange()
        {
            Updated?.Invoke();

            InnerUpdate();
        }
        bool InnerUpdate()
        {
            if (!_bindingBuild)
                return false;

            if(_getValue == null)
                BuildSource();
            
            if (_getValue == null || _setValue == null)
                return false;

            _setValue(_getValue());
            return true;
        }

        #endregion

        public bool Update()
        {
            _bindingBuild = true;
            if(IsTwoSided)
                InnerTargetUpdate();
            return InnerUpdate();
        }

        #region target subscription

        private Action<object> _setSourceValue;
        private Func<object> _getTargetValue;
        private object _lastTargetValue;
        
        void InnerTargetUpdate()
        {
            var oldValue = _lastTargetValue;
            var newValue = _lastTargetValue = _getTargetValue();
            if(newValue == oldValue)
                return;
            
            if(newValue != null && newValue.Equals(oldValue))
                return;

            _setSourceValue(newValue);
        }

        public bool IsTwoSided => _targetSubscription != null;
        private IDisposable _targetSubscription;

        public IDisposable SetTwoSided()
        {
            if (_targetSubscription != null) 
                return _targetSubscription;
            
            var cts = new CancellationTokenSource();
            AsyncExtension.RunEveryLateUpdate(InnerTargetUpdate, cts.Token);
            _targetSubscription = new ActionDisposable(() =>
            {
                _targetSubscription = null;
                cts.Cancel();
            });

            return _targetSubscription;
        }

        #endregion
    }
}