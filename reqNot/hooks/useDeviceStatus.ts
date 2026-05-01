import { useEffect, useState, useCallback } from 'react';
import firestore from '@react-native-firebase/firestore';
import auth from '@react-native-firebase/auth';
import { DeviceStatus } from '@/constants/DeviceStatus';

const docRef = firestore().collection('reqNot').doc('actual');

export function useDeviceStatus() {
  const [isOn, setIsOn] = useState<DeviceStatus | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [lastChecked, setLastChecked] = useState<Date | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let firestoreUnsub: (() => void) | null = null;

    const authUnsub = auth().onAuthStateChanged((user) => {
      if (firestoreUnsub) {
        firestoreUnsub();
        firestoreUnsub = null;
      }

      if (!user) {
        setError('Brak dostępu — zaloguj się');
        setIsChecking(false);
        return;
      }

      firestoreUnsub = docRef.onSnapshot(
        (snapshot) => {
          setError(null);
          const data = snapshot.data();
          if (!data) return;
          if (typeof data.works === 'number') setIsOn(data.works as DeviceStatus);
          else if (typeof data.works === 'boolean') setIsOn(data.works ? DeviceStatus.On : DeviceStatus.Off);
          if (data.check === false) setIsChecking(false);
          if (data.lastChecked && typeof data.lastChecked.toDate === 'function') {
            setLastChecked(data.lastChecked.toDate());
          }
        },
        () => {
          setError('Brak dostępu — zaloguj się');
          setIsChecking(false);
        }
      );
    });

    return () => {
      authUnsub();
      if (firestoreUnsub) firestoreUnsub();
    };
  }, []);

  const check = useCallback(async () => {
    setIsChecking(true);
    setError(null);
    try {
      await docRef.set({ check: true }, { merge: true });
    } catch {
      setError('Brak dostępu — zaloguj się');
      setIsChecking(false);
    }
  }, []);

  return { isOn, isLoading: isChecking, check, lastChecked, error };
}
