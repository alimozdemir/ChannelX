import './css/site.css';
import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import axios from 'axios';
import VeeValidate from 'vee-validate';

Vue.use(VueRouter);
Vue.use(VeeValidate);

const routes = [
    {
        path : '',
        component : require('./layouts/app.vue.html'),
        children : [
            { path: '/', component: require('./components/home/home.vue.html') },
            { path: '/counter', component: require('./components/counter/counter.vue.html') },
            { path: '/fetchdata', component: require('./components/fetchdata/fetchdata.vue.html') },
            { path: '/channel/create', component : require('./components/channel/create.vue.html') },
            { name: '/channel/open', path: '/channel/open/:id', component: require('./components/channel/open.vue.html'), props: true  },
            { path: '/historypage', component : require('./components/historypage/historypage.vue.html') }
            
        ]
    },
    {   
        path : '', 
        component : require('./layouts/login.vue.html'),
        children : [
            { path : '/login', component : require('./components/login/login.vue.html') }
        ]
    }
]


var router = new VueRouter({ mode: 'history', routes: routes });

// add the auth key to every request of axios
axios.interceptors.request.use(request => {
    let auth = localStorage.getItem('auth');

    if(auth !== undefined && auth)
        request.headers.common['Authorization'] = 'Bearer ' + auth;

    return request;
})

axios.interceptors.response.use(response => {
    return response;
}, error => {
    // if unauthorized request then remove the auth key and route to login page
    if(error.response.status === 401){
        localStorage.removeItem('auth');
        router.push('/login');
    }
});


router.beforeEach((to, from, next) => {
    let auth = localStorage.getItem('auth');
    
    // if the auth key is exists then go foward
    // otherwise go login page
    if(auth !== undefined && auth){
        next();
    }
    else if(to.path === '/login') {
        next();
    }
    else {
        next('/login');
    }

});

new Vue({
    el: '#app-root',
    router: router,
    render: h => h(require('./layouts/main.vue.html'))
});
