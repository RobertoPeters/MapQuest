window.gpsHelper = {
    watchId: null,

    isGeolocationAvailable: function () {
        return 'geolocation' in navigator;
    },

    getCurrentPosition: function (dotNetRef) {
        if (!navigator.geolocation) {
            dotNetRef.invokeMethodAsync('OnLocationError', 'Geolocation is not supported by this browser.');
            return;
        }

        navigator.geolocation.getCurrentPosition(
            (position) => {
                dotNetRef.invokeMethodAsync('OnLocationReceived', {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy || null,
                    altitude: position.coords.altitude || null,
                    heading: position.coords.heading || null,
                    speed: position.coords.speed || null,
                    timestamp: Number(position.timestamp)
                });
            },
            (error) => {
                dotNetRef.invokeMethodAsync('OnLocationError', error.message);
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    },

    startWatchingPosition: function (dotNetRef) {
        if (!navigator.geolocation) {
            dotNetRef.invokeMethodAsync('OnLocationError', 'Geolocation is not supported by this browser.');
            return false;
        }

        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
        }

        this.watchId = navigator.geolocation.watchPosition(
            (position) => {
                dotNetRef.invokeMethodAsync('OnLocationReceived', {
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy || null,
                    altitude: position.coords.altitude || null,
                    heading: position.coords.heading || null,
                    speed: position.coords.speed || null,
                    timestamp: Number(position.timestamp)
                });
            },
            (error) => {
                dotNetRef.invokeMethodAsync('OnLocationError', error.message);
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );

        return true;
    },

    stopWatchingPosition: function () {
        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
            return true;
        }
        return false;
    }
};
