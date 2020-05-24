package com.hideakin.textsearch.index.configuration;

import org.springframework.context.annotation.Configuration;
import org.springframework.format.FormatterRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

import com.hideakin.textsearch.index.converter.StringToSearchOptionsConverter;

@Configuration
public class WebConfig implements WebMvcConfigurer {

	@Override
    public void addFormatters(FormatterRegistry registry) {
        registry.addConverter(new StringToSearchOptionsConverter());
    }

}
