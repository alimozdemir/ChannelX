import Vue from 'vue';
import './historypage.css'
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import swal from 'sweetalert';
import resultModel from '../../models/resultModel';
import moment from 'moment';


interface historyModel {
    id: number,
    title: string,
    endAt: Date,
    createdAt : Date,
    duration: number,
    engagedUsersName: string[]
    
}

@Component/*({
    filters: {
        duration : function(createdAt:Date, endAt:Date){
            return moment(value).fromNow();
        }
    }
})*/
export default class HistoryPageComponent extends Vue {

    modelList: historyModel[] = [];

    async mounted(){
        await this.func();
    }
    async func() {
        let result = await axios.get('/api/Channel/HistoryPage')
        if(result.status == 200) {
            let data = result.data as historyModel[];
            this.modelList = data;
        }
        else{
            swal({ text: "something went wrong", icon: "error" })
        }
    
    }
}