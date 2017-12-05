import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import { UserStore } from '../../stores/userState'
import './topmenu.css';
import UserComponent from '../user/user';

@Component({
    computed : {
        getUserName(){
            return UserStore.readUserName(this.$store);
        }
    }
})
export default class TopMenuComponent extends Vue {

    mounted() {

        
    }
    
    async showMe(){
        let id = UserStore.readUserId(this.$store);
        let popup = new UserComponent(id);

        await popup.show();
    }

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
