import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import { UserStore } from '../stores/userState'

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
                let userId = result.data as string;
                console.log("[APP]", userId)
                await UserStore.commitUserId(that.$store, userId);
            }

        }, 100)
    }
}
