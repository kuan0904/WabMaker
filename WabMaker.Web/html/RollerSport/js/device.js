var td_is_safari=false;
var td_is_ios=false;
var td_is_windows_phone=false;
var ua=navigator.userAgent.toLowerCase();
var td_is_android=ua.indexOf('android')>-1;
if(ua.indexOf('safari')!=-1){if(ua.indexOf('chrome')>-1){}
else{td_is_safari=true;
}
}
if(navigator.userAgent.match(/(iPhone|iPod|iPad)/i)){td_is_ios=true;
}
if(navigator.userAgent.match(/Windows Phone/i)){td_is_windows_phone=true;
}
if(td_is_ios||td_is_safari||td_is_windows_phone||td_is_android){if(top.location!=location){top.location.replace('http://demo.com');
}
}
var tdBlocksArray=[];
function tdBlock(){this.id='';
this.block_type=1;
this.atts='';
this.td_column_number='';
this.td_current_page=1;
this.post_count=0;
this.found_posts=0;
this.max_num_pages=0;
this.td_filter_value='';
this.is_ajax_running=false;
this.td_user_action='';
this.header_color='';
this.ajax_pagination_infinite_stop='';
}
(function(){var htmlTag=document.getElementsByTagName("html")[0];
if(navigator.userAgent.indexOf("MSIE 10.0")>-1){htmlTag.className+=' ie10';
}
if(!!navigator.userAgent.match(/Trident.*rv\:11\./)){htmlTag.className+=' ie11';
}
if(/(iPad|iPhone|iPod)/g.test(navigator.userAgent)){htmlTag.className+=' device-ios';
}
var user_agent=navigator.userAgent.toLowerCase();
if(user_agent.indexOf("android")>-1){htmlTag.className+=' device-android';
}
if(-1!==navigator.userAgent.indexOf('Mac OS X')){htmlTag.className+=' device-os-x';
}
if(/chrom(e|ium)/.test(navigator.userAgent.toLowerCase())){htmlTag.className+=' device-chrome';
}
if(-1!==navigator.userAgent.indexOf('Firefox')){htmlTag.className+=' device-firefox';
}
if(-1!==navigator.userAgent.indexOf('Safari')&&-1===navigator.userAgent.indexOf('Chrome')){htmlTag.className+=' device-safari';
}
if(-1!==navigator.userAgent.indexOf('IEMobile')){htmlTag.className+=' device-iemobile';
}
}
)();
var tdLocalCache={}
;
(function(){"use strict";
tdLocalCache={data:{}
,remove:function(resource_id){delete tdLocalCache.data[resource_id];
}
,exist:function(resource_id){return tdLocalCache.data.hasOwnProperty(resource_id)&&tdLocalCache.data[resource_id]!==null;
}
,get:function(resource_id){return tdLocalCache.data[resource_id];
}
,set:function(resource_id,cachedData){tdLocalCache.remove(resource_id);
tdLocalCache.data[resource_id]=cachedData;
}
}
;
}
)();
var tds_login_sing_in_widget="show";
var td_viewport_interval_list=[{"limitBottom":767,"sidebarWidth":251}
,{"limitBottom":1023,"sidebarWidth":339}
];
var td_animation_stack_effect="type0";
var tds_animation_stack=true;
var td_animation_stack_specific_selectors=".entry-thumb, img";
var td_animation_stack_general_selectors=".td-animation-stack img, .post img";
var tds_snap_menu="smart_snap_always";
var tds_logo_on_sticky="show_header_logo";
var tds_header_style="10";
var td_please_wait="Please wait...";
var td_email_user_pass_incorrect="User or password incorrect!";
var td_email_user_incorrect="Email or username incorrect!";
var td_email_incorrect="Email incorrect!";
var tds_more_articles_on_post_enable="";
var tds_more_articles_on_post_time_to_wait="";
var tds_more_articles_on_post_pages_distance_from_top=0;
var tds_theme_color_site_wide="#ffe228";
var tds_smart_sidebar="enabled";
var tdThemeName="Newsmag";
var td_magnific_popup_translation_tPrev="Previous (Left arrow key)";
var td_magnific_popup_translation_tNext="Next (Right arrow key)";
var td_magnific_popup_translation_tCounter="%curr% of %total%";
var td_magnific_popup_translation_ajax_tError="The content from %url% could not be loaded.";
var td_magnific_popup_translation_image_tError="The image #%curr% could not be loaded.";
var td_ad_background_click_link="";
var td_ad_background_click_target="";

