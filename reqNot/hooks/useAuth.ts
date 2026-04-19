import { useEffect, useState } from 'react';
import auth, { FirebaseAuthTypes } from '@react-native-firebase/auth';
import { GoogleSignin, statusCodes } from '@react-native-google-signin/google-signin';

const WEB_CLIENT_ID = '121109249006-c6935okrrhim9br70r52f94021eebdui.apps.googleusercontent.com';

let configured = false;
function ensureConfigured() {
  if (!configured) {
    GoogleSignin.configure({ webClientId: WEB_CLIENT_ID });
    configured = true;
  }
}

export function useAuth() {
  const [user, setUser] = useState<FirebaseAuthTypes.User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const unsubscribe = auth().onAuthStateChanged((u) => {
      setUser(u);
      setLoading(false);
    });
    return unsubscribe;
  }, []);

  const signIn = async () => {
    try {
      ensureConfigured();
      await GoogleSignin.hasPlayServices();
      const response = await GoogleSignin.signIn();
      const idToken = response.data?.idToken;
      if (!idToken) throw new Error('No idToken');
      const credential = auth.GoogleAuthProvider.credential(idToken);
      await auth().signInWithCredential(credential);
    } catch (error: any) {
      if (error.code !== statusCodes.SIGN_IN_CANCELLED) throw error;
    }
  };

  const signOut = async () => {
    ensureConfigured();
    await GoogleSignin.signOut();
    await auth().signOut();
  };

  return { user, loading, signIn, signOut };
}
