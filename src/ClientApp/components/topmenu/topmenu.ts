import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import './topmenu.css';

@Component
export default class TopMenuComponent extends Vue {
    logout(){
        localStorage.removeItem('auth');
        this.$router.push('/login');
    }
}
