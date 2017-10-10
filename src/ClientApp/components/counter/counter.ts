import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios, { AxiosRequestConfig } from 'axios';

@Component
export default class CounterComponent extends Vue {
    currentcount: number = 0;
    msg : string = "";
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
        this.msg = testAuth.data;
        console.log(testAuth);
    }
}
