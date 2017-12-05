import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import axios from 'axios';
import './user.css';
import resultModel from '../../models/resultModel';

interface getUser {
    userId: string,
    userName: string,
    firstNameAndLastName: string,
    email: string
}

export default class UserComponent {
    /**
     *
     */
    userId: string;
    constructor(id: string) {
        this.userId = id;
    }

    async show() {
        console.log("show")
        var response = await axios.get('/api/Account/GetUser', {
            params:
                {
                    id: this.userId
                }
        });
        if (response.status === 200)
        {
            var result = response.data as resultModel;

            if(result.succeeded)
            {
                var data = result.data as getUser;
                await swal({
                    title: data.userName + (data.firstNameAndLastName ? '(' + data.firstNameAndLastName + ')' : ''),
                    text: data.email
                })
            }
        }
    }
}
