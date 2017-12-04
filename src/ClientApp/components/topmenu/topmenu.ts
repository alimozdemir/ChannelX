import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import { UserStore } from '../../stores/userState'
import './topmenu.css';

@Component
export default class TopMenuComponent extends Vue {
    logout(){
        
        UserStore.commitUserId(this.$store, "");
        UserStore.commitAuthKey(this.$store, "");

        // to stop infinite interval
        let id = localStorage.getItem('interval');

        if (id != null)
        {
            let interval = parseInt(id);
            localStorage.removeItem('interval');
            clearInterval(interval);            
        }

        this.$router.push('/login');
    }
}
