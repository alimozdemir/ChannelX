import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import { UserStore } from '../stores/userState'

interface userData {
    userId: string,
    userName: string
}


@Component({
    components: {
        MenuComponent: require('../components/navmenu/navmenu.vue.html'),
        TopMenu : require('../components/topmenu/topmenu.vue.html')
    }
})
export default class AppComponent extends Vue {
    async mounted() {
        var that = this
        setTimeout(async function(){

            var result = await axios.get('/Home/Test');
            if(result.status !== 200){
                //this.$router.push('/login');
            }
            else{
                let user = result.data as userData;
                
                await UserStore.commitUserId(that.$store, user.userId);
                await UserStore.commitUserName(that.$store, user.userName);
            }

        }, 100)
    }
}
