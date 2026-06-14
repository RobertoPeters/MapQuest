window.gpsHelper = {
    watchId: null,
    wakeLock: null,
    visibilityHandler: null,

    isGeolocationAvailable: function () {
        return 'geolocation' in navigator;
    },

    requestWakeLock: async function () {
        if (!('wakeLock' in navigator)) {
            console.warn('Wake Lock is not supported on this browser.');
            return;
        }
        try {
            this.wakeLock = await navigator.wakeLock.request('screen');
            console.info('Wake Lock is active');
        } catch (err) {
            console.error(`Wake Lock failed: ${err.name}, ${err.message}`);
        }
    },

    releaseWakeLock: function () {
        if (this.wakeLock !== null) {
            this.wakeLock.release()
                .then(() => {
                    this.wakeLock = null;
                    console.info('Wake Lock released');
                })
                .catch((err) => {
                    console.error(`Wake Lock release failed: ${err.message}`);
                });
        }
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

        this.startWakeLockListener();
        this.requestWakeLock();

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
        this.stopWakeLockListener();
        this.releaseWakeLock();

        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
            return true;
        }
        return false;
    },

    startWakeLockListener: function () {
        if (this.visibilityHandler) return;
        this.visibilityHandler = async () => {
            if (this.watchId !== null && document.visibilityState === 'visible') {
                await this.requestWakeLock();
            }
        };
        document.addEventListener('visibilitychange', this.visibilityHandler);
    },

    stopWakeLockListener: function () {
        if (this.visibilityHandler) {
            document.removeEventListener('visibilitychange', this.visibilityHandler);
            this.visibilityHandler = null;
        }
    }
};
