import { useEffect, useState, useCallback } from 'react';
import firestore from '@react-native-firebase/firestore';

const docRef = firestore().collection('reqNot').doc('actual');

export function useDeviceStatus() {
  const [isOn, setIsOn] = useState<boolean | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [lastChecked, setLastChecked] = useState<Date | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const unsubscribe = docRef.onSnapshot(
      (snapshot) => {
        setError(null);
        const data = snapshot.data();
        if (!data) return;
        if (typeof data.works === 'boolean') setIsOn(data.works);
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
    return unsubscribe;
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
