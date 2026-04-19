import { useEffect } from 'react';
import * as Notifications from 'expo-notifications';
import messaging from '@react-native-firebase/messaging';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: false,
  }),
});

export function usePushNotifications() {
  useEffect(() => {
    async function register() {
      const { status } = await Notifications.requestPermissionsAsync();
      if (status !== 'granted') return;

      await messaging().subscribeToTopic('rekuperator');
    }

    register();

    const foregroundSub = Notifications.addNotificationReceivedListener(() => {});
    const responseSub = Notifications.addNotificationResponseReceivedListener(() => {});

    return () => {
      foregroundSub.remove();
      responseSub.remove();
    };
  }, []);
}
