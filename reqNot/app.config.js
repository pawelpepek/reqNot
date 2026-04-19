const fs = require('fs');

if (process.env.GOOGLE_SERVICES_JSON) {
  fs.copyFileSync(process.env.GOOGLE_SERVICES_JSON, './google-services.json');
}

export default {
  expo: {
    name: 'reqNot',
    slug: 'reqNot',
    version: '1.0.0',
    orientation: 'portrait',
    icon: './assets/images/icon.png',
    scheme: 'reqnot',
    userInterfaceStyle: 'automatic',
    newArchEnabled: true,
    ios: {
      supportsTablet: true,
      googleServicesFile: './GoogleService-Info.plist',
    },
    android: {
      googleServicesFile: './google-services.json',
      package: 'req.app',
      adaptiveIcon: {
        backgroundColor: '#E6F4FE',
        foregroundImage: './assets/images/android-icon-foreground.png',
        backgroundImage: './assets/images/android-icon-background.png',
        monochromeImage: './assets/images/android-icon-monochrome.png',
      },
      edgeToEdgeEnabled: true,
      predictiveBackGestureEnabled: false,
    },
    web: {
      output: 'static',
      favicon: './assets/images/favicon.png',
    },
    plugins: [
      'expo-router',
      'expo-notifications',
      [
        'expo-splash-screen',
        {
          image: './assets/images/splash-icon.png',
          imageWidth: 200,
          resizeMode: 'contain',
          backgroundColor: '#ffffff',
          dark: {
            backgroundColor: '#000000',
          },
        },
      ],
      '@react-native-firebase/app',
      '@react-native-firebase/messaging',
    ],
    experiments: {
      typedRoutes: true,
      reactCompiler: true,
    },
    extra: {
      router: {},
      eas: {
        projectId: 'c9ad9f8a-2f22-49cc-b405-8c8d0c9c657d',
      },
    },
    owner: process.env.EXPO_OWNER,
  },
};
