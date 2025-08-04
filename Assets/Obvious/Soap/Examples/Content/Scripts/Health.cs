using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Obvious.Soap.Example
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float _currentHealth;
        public float CurrentHealth => _currentHealth;

        [SerializeField] private float _maxHealth;
        public float MaxHealth => _maxHealth;

        public event Action<int> OnDamaged;
        public event Action<int> OnCriticalDamaged;
        public event Action<int> OnHealed;
        public event Action OnDeath;

        private bool _isDead = false;
        private bool _isCritical = false; // Example of critical state, can be used for special damage handling

        private void OnEnable()
        {
            _currentHealth = _maxHealth;
        }

        private void OnDamagedHandler(float value)
        {
            if (_currentHealth <= 0f && !_isDead)
                OnDeathHandler();
            else
            {
                if (_isCritical)
                {
                    OnCriticalDamaged?.Invoke(Mathf.RoundToInt(value));
                    _isCritical = false; // Reset critical state after handling
                }
                else
                {
                    OnDamaged?.Invoke(Mathf.RoundToInt(value));
                }
            }
        }

        private void OnHealedHandler(float value)
        {
            OnHealed?.Invoke(Mathf.RoundToInt(value));
        }

        private void OnDeathHandler()
        {
            _isDead = true;
            OnDeath?.Invoke();
        }

        //if you don't want to modify directly the health, you can also do it like this
        //Used in the Event example.
        [UsedImplicitly]
        public void TakeDamage(int amount)
        {
            _isCritical = false; // Reset critical state on normal damage
            _currentHealth -= amount;
            OnDamagedHandler(amount);
        }

        public void TakeCriticalDamage(int amount)
        {
            _isCritical = true; // Set critical state for special handling
            _currentHealth -= amount; // Example of critical damage
            OnDamagedHandler(amount);
        }

        public void Heal(int amount)
        {
            _currentHealth += amount;
            OnHealedHandler(amount);
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false; // Reset dead state
        }
    }
}