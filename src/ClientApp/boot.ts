import './css/site.css';
import './css/loading.css';
import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import axios from 'axios';
import VeeValidate from 'vee-validate';
import Vuex, { Store } from 'vuex'
import { createStore, State } from './stores/store'
import { UserStore } from './stores/userState'

Vue.use(VueRouter);
Vue.use(VeeValidate);
Vue.use(Vuex);

let stores = createStore();

const routes = [
    {
        path: '',
        component: require('./layouts/app.vue.html'),
        children: [
            { path: '/', component: require('./components/home/home.vue.html') },
            { path: '/channel/create', component: require('./components/channel/create.vue.html') },
            { name: '/channel/open', path: '/channel/open/:id', component: require('./components/channel/open.vue.html'), props: true },
            { name: '/sh', path: '/sh/:hash', component: require('./components/channel/open.vue.html'), props: true },
            { path: '/historypage', component: require('./components/historypage/historypage.vue.html') }
        ]
    },
    {
        path: '',
        component: require('./layouts/login.vue.html'),
        children: [
            { path: '/login', component: require('./components/login/login.vue.html') },
            { path: '/register', component: require('./components/register/register.vue.html') },
            { name: '/resetpass', path: '/resetpass/:hash', component: require('./components/resetpassword/resetpassword.vue.html'), props: true },
            { path: '/forgotpassword', component: require('./components/forgotpassword/forgotpassword.vue.html') },
        ]
    }
]

var router = new VueRouter({ mode: 'history', routes: routes });

var app = new Vue({
    el: '#app-root',
    router: router,
    render: h => h(require('./layouts/main.vue.html')),
    store: stores
});

// add the auth key to every request of axios
axios.interceptors.request.use(request => {
    // let auth = localStorage.getItem('auth');
    // make sure auth is dispatched
    UserStore.dispatchAuthKey(app.$store);

    let auth = UserStore.readAuthKey(app.$store);
    let userId = UserStore.readUserId(app.$store)

    if (auth !== undefined && auth)
        request.headers.common['Authorization'] = 'Bearer ' + auth;

    return request;
})

axios.interceptors.response.use(response => {
    return response;
}, error => {
    // if unauthorized request then remove the auth key and route to login page
    if (error.response.status === 401) {
        UserStore.commitAuthKey(app.$store, "");
        UserStore.commitUserId(app.$store, "");
        let interval = UserStore.readInterval(app.$store);

        if (interval !== "") {
            UserStore.commitInterval(app.$store, "");
            clearInterval(parseInt(interval));
        }

        router.push('/login');
    }
});

router.beforeEach((to, from, next) => {
    let auth = UserStore.readAuthKey(app.$store);
    // if the auth key is exists then go forward
    // otherwise go login page
    if (auth !== undefined && auth !== "") {
        next();
    }
    else if (to.path === '/login' || to.path === '/register' || to.path === '/forgotpassword') {
        next();
    }
    else {
        next('/login');
    }

});



