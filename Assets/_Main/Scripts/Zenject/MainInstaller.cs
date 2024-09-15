using UnityEngine;
using WhereIsMyWife.Controllers;
using WhereIsMyWife.Databases;
using WhereIsMyWife.Managers;
using WhereIsMyWife.Managers.Properties;
using WhereIsMyWife.Player.State;
using WhereIsMyWife.Player.StateMachine;
using Zenject;

namespace WhereIsMyWife
{
    public class MainInstaller : MonoInstaller
    {
        [SerializeField] private PlayerProperties _playerProperties;
        
        public override void InstallBindings()
        {
            InstallScriptableObjectBindings();
            InstallDatabaseBindings();
            InstallManagerBindings();
            InstallActionMethods();
            InstallPlayerStates();
        }

        private void InstallManagerBindings()
        {
            Container.BindInterfacesTo<PlayerManager>().AsSingle();
            Container.BindInterfacesTo<InputEventManager>().AsSingle();
        }

        private void InstallDatabaseBindings()
        {
            Container.BindInterfacesTo<OptionsDatabase>().AsSingle();
            Container.BindInterfacesTo<PlayerProgressionDatabase>().AsSingle();
            Container.BindInterfacesTo<StatsDatabase>().AsSingle();
        }

        private void InstallScriptableObjectBindings()
        {
            InstallPlayerProperties();
        }

        private void InstallPlayerProperties()
        {
            InstallPlayerPropertiesModules();

            Container.BindInterfacesTo<PlayerProperties>().FromScriptableObject(_playerProperties).AsSingle();
        }

        private void InstallPlayerPropertiesModules()
        {
            Container.BindInterfacesTo<PlayerJumpProperties>().FromScriptableObject(_playerProperties.JumpProperties).AsSingle();
            Container.BindInterfacesTo<PlayerDashProperties>().FromScriptableObject(_playerProperties.DashProperties).AsSingle();
            Container.BindInterfacesTo<PlayerMovementProperties>().FromScriptableObject(_playerProperties.MovementProperties).AsSingle();
            Container.BindInterfacesTo<PlayerGravityProperties>().FromScriptableObject(_playerProperties.GravityProperties).AsSingle();
            Container.BindInterfacesTo<PlayerCheckProperties>().FromScriptableObject(_playerProperties.CheckProperties).AsSingle();
        }

        private void InstallActionMethods()
        {
            Container.BindInterfacesTo<RunningMethods>().AsSingle();
            Container.BindInterfacesTo<JumpingMethods>().AsSingle();
        }

        private void InstallPlayerStates()
        {
            Container.BindInterfacesTo<PlayerMovementState>().AsSingle();
            Container.BindInterfacesTo<PlayerDashState>().AsSingle();
            Container.BindInterfacesTo<PlayerWallHangState>().AsSingle();
        }
    }
}
