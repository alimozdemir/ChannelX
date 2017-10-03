import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios, { AxiosRequestConfig } from 'axios';

@Component
export default class CounterComponent extends Vue {
    currentcount: number = 0;
    
    incrementCounter() {
        console.log(this);
        this.currentcount++;
    }

    async test() {

        // Get the token
        var token = await axios.get('/Home/Login');

        console.log(token);
        
        // Check for basic auth
        var testAuth = await axios.get('/Home/Test', { headers : { 'Authorization' : 'Bearer ' + token.data } });

        console.log(testAuth.data);
    }
}
