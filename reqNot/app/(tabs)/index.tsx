import { ActivityIndicator, Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { useState } from 'react';
import { Ionicons } from '@expo/vector-icons';
import { useDeviceStatus } from '@/hooks/useDeviceStatus';
import { useAuth } from '@/hooks/useAuth';
import { DeviceStatus } from '@/constants/DeviceStatus';

export default function HomeScreen() {
  const { isOn, isLoading, check, lastChecked, error } = useDeviceStatus();
  const { user, loading: authLoading, signIn, signOut } = useAuth();
  const [authBusy, setAuthBusy] = useState(false);

  function renderIcon() {
    if (isOn === null)                return <Ionicons name="power" size={120} color="#9ca3af" />;
    if (isOn === DeviceStatus.Unknown) return <Ionicons name="help-circle-outline" size={120} color="#f59e0b" />;
    if (isOn === DeviceStatus.On)      return <Ionicons name="power" size={120} color="#22c55e" />;
    return                                    <Ionicons name="power" size={120} color="#ef4444" />;
  }

  const handleSignIn = async () => {
    setAuthBusy(true);
    try {
      await signIn();
    } catch {
      Alert.alert('Błąd logowania', 'Nie udało się zalogować. Spróbuj ponownie.');
    } finally {
      setAuthBusy(false);
    }
  };

  const handleSignOut = async () => {
    setAuthBusy(true);
    try {
      await signOut();
    } catch {
      Alert.alert('Błąd', 'Nie udało się wylogować. Spróbuj ponownie.');
    } finally {
      setAuthBusy(false);
    }
  };

  function renderAuthButton() {
    if (authLoading || authBusy) {
      return <ActivityIndicator size="small" color="#6b7280" style={styles.authSpinner} />;
    }
    if (user) {
      return (
        <View style={styles.authRow}>
          <Text style={styles.authEmail} numberOfLines={1}>{user.email}</Text>
          <Pressable style={styles.authButton} onPress={handleSignOut}>
            <Text style={styles.authButtonText}>Wyloguj</Text>
          </Pressable>
        </View>
      );
    }
    return (
      <Pressable style={styles.authButton} onPress={handleSignIn}>
        <Text style={styles.authButtonText}>Zaloguj przez Google</Text>
      </Pressable>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.center}>
        {isLoading
          ? <ActivityIndicator size="large" color="#6b7280" />
          : renderIcon()
        }
        {error
          ? <Text style={styles.error}>{error}</Text>
          : <Text style={styles.lastChecked}>
              Ostatnie sprawdzenie:{' '}
              {lastChecked?.toLocaleString('pl-PL', { dateStyle: 'short', timeStyle: 'short' }) ?? '–'}
            </Text>
        }
      </View>
      <View style={styles.buttons}>
        <Pressable
          style={[styles.button, isLoading && styles.buttonDisabled]}
          onPress={check}
          disabled={isLoading}>
          <Text style={styles.buttonText}>Sprawdź</Text>
        </Pressable>
        {renderAuthButton()}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'space-between',
    paddingVertical: 48,
    paddingHorizontal: 24,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  buttons: {
    gap: 12,
  },
  button: {
    backgroundColor: '#3b82f6',
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: 'center',
  },
  buttonDisabled: {
    opacity: 0.5,
  },
  lastChecked: {
    marginTop: 16,
    fontSize: 13,
    color: '#6b7280',
  },
  error: {
    marginTop: 16,
    fontSize: 13,
    color: '#ef4444',
  },
  buttonText: {
    color: '#ffffff',
    fontSize: 18,
    fontWeight: '600',
  },
  authButton: {
    backgroundColor: '#f3f4f6',
    borderRadius: 12,
    paddingVertical: 14,
    alignItems: 'center',
  },
  authButtonText: {
    color: '#374151',
    fontSize: 15,
    fontWeight: '500',
  },
  authRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  authEmail: {
    flex: 1,
    fontSize: 13,
    color: '#6b7280',
  },
  authSpinner: {
    paddingVertical: 14,
  },
});
