import './css/site.css';
import 'bootstrap';
import Vue from 'vue';
import VueRouter from 'vue-router';
import axios from 'axios';

Vue.use(VueRouter);

const routes = [
    
    {
        path : '',
        component : require('./layouts/app.vue.html'),
        children : [
            { path: '/', component: require('./components/home/home.vue.html') },
            { path: '/counter', component: require('./components/counter/counter.vue.html') },
            { path: '/fetchdata', component: require('./components/fetchdata/fetchdata.vue.html') }
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

new Vue({
    el: '#app-root',
    router: router,
    render: h => h(require('./layouts/main.vue.html'))
});
